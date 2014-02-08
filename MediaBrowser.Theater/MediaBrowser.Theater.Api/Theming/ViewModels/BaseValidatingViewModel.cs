using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.Theming.ViewModels
{
    /// <summary>
    /// A base view model class which also provides validation logic.
    /// By default, validation is applied to DataAnnotation validation attributes on properties.
    /// </summary>
    public abstract class BaseValidatingViewModel
        : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly ConcurrentDictionary<string, List<string>> _errors = new ConcurrentDictionary<string, List<string>>();
        private readonly AsyncLock _validationLock = new AsyncLock();
        
        public IEnumerable GetErrors(string propertyName)
        {
            List<string> errorsForName;
            _errors.TryGetValue(propertyName, out errorsForName);
            return errorsForName;
        }

        public virtual bool HasErrors
        {
            get { return _errors.Any(kv => kv.Value != null && kv.Value.Count > 0); }
        }

        protected ConcurrentDictionary<string, List<string>> ValidationErrors
        {
            get { return _errors; }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
        {
            EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(propertyName, true);
        }

        protected virtual async void OnPropertyChanged(string propertyName, bool validate)
        {
            base.OnPropertyChanged(propertyName);
            if (validate) {
                await Validate();
            }
        }

        public virtual Task<bool> Validate()
        {
            return Task.Run(async () => {
                using (await _validationLock.LockAsync()) {
                    var validationContext = new ValidationContext(this, null, null);
                    var validationResults = new List<ValidationResult>();
                    Validator.TryValidateObject(this, validationContext, validationResults, true);

                    foreach (var kv in _errors.ToList()) {
                        if (validationResults.All(r => r.MemberNames.All(m => m != kv.Key))) {
                            List<string> outLi;
                            _errors.TryRemove(kv.Key, out outLi);
                            OnErrorsChanged(new DataErrorsChangedEventArgs(kv.Key));
                        }
                    }

                    IEnumerable<IGrouping<string, ValidationResult>> q = from r in validationResults
                                                                         from m in r.MemberNames
                                                                         group r by m
                                                                         into g
                                                                         select g;

                    foreach (var prop in q) {
                        List<string> messages = prop.Select(r => r.ErrorMessage).ToList();

                        if (_errors.ContainsKey(prop.Key)) {
                            List<string> outLi;
                            _errors.TryRemove(prop.Key, out outLi);
                        }

                        _errors.TryAdd(prop.Key, messages);
                        OnErrorsChanged(new DataErrorsChangedEventArgs(prop.Key));
                    }

                    OnPropertyChanged("HasErrors", false);

                    return !HasErrors;
                }
            });
        }
    }
}