using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemDetailsViewModel
        : BaseViewModel
    {
        private readonly BaseItemDto _item;
        private readonly IEnumerable<IItemDetailSection> _sections;

        public ItemDetailsViewModel(BaseItemDto item, IEnumerable<IItemDetailSection> sections)
        {
            _item = item;
            _sections = sections.ToList();
        }

        public IEnumerable<IItemDetailSection> Sections
        {
            get { return _sections; }
        }
    }
}