--[[
    See script details on https://github.com/kevinlekiller/mpv_scripts
    
    Valid --script-opts are (they are all optional):
    refreshrate-enabled=false      true/false - Use nircmd to change the refresh rate of your monitor.
    refreshrate-rates="60"        String     - String of refresh rates your monitor supports and you want to use, separated by semicolons.
                                             Nircmd seems to prefer rounded numbers, 72 instead of 71.92 for example.
                                             Examples: refreshrate-rates="60" | refreshrate-rates="50;60;72"
                                             Note if you want a rate to be prefered over another, for example the video is 24hz
                                             and your rates are "100;120;144", but you want 144hz instead of 120hz for 24fps videos, change the
                                             order to refreshrate-rates="100;144;120", then 144hz will take precendence over 120hz.
    refreshrate-pause            true/false - Pause video while switching display modes.
                                Number     - If you set this a number, it will pause for that amount of seconds.
    refreshrate-method="always"   String     - Set how often to check if framerate conforms to settings
					     "always": (original behaviour) contstantly checks during playback
					     "start": only does initial change at start
						 
    Example: mpv file.mkv --script-opts=refreshrate-enabled=true,refreshrate-minspeed=0.8
--]]
local _global = {
    utils = require 'mp.utils',
    lastDrr = 0,
    next = next,
    trigger_refreshFound = false,
	initial_start = false,
	rates_actual = "";
	rate_initial = 0;
	item_count = 0,
}

function round(number)
    return math.floor(number + 0.5)
end

function sleep(n)  -- seconds
	local clock = os.clock
	local t0 = clock()
	while 
		clock() - t0 <= n do 
	end
end

function getOptions()
    _global.options = {
        ["enabled"]    = false,
        ["program"]   = "refreshrate",
        ["monitor"]   = 0,
        ["rates"]     = "",
        ["pause"]    = 3,
		["method"]	  = "once",
		["theme"] = false,
    }
    for key, value in pairs(_global.options) do
        local opt = mp.get_opt("refreshrate-" .. key)
        if (opt ~= nil) then
            if ((key == "enabled" or key == "skipmultiples" or key == "osd" or key == "estfps" or key == "theme") and opt == "true") then
                _global.options[key] = true
            elseif (key == "pause") then
                local test = tonumber(opt)
                if (test ~= nil) then
                    _global.options[key] = test
                end
            else
                _global.options[key] = opt
            end
        end
    end
end
getOptions()

function change_rate(rates)
	local data = _global.utils.subprocess({
		["cancellable"] = false,
		["args"] = {
			[1] = _global.options["program"],
			[2] = "change",
			[3] = _global.options["monitor"],
			[4] = rates
		}
	})
	mp.msg.info(tostring(data.stdout))
	return tonumber(tostring(string.gsub(data.stdout:gsub('%W',''),"CurrentRefreshRate","")))
end

function check_rates(actual, computed)
	for x in string.gmatch(actual,'([^;]+)')
	do
		if x==computed then
			return true
		end
	end
	return false
end

function possible_rate()
	mp.msg.info("Querying Device Refresh Rates...")
	local data = _global.utils.subprocess({
		["cancellable"] = false,
		["args"] = {
			[1] = _global.options["program"],
			[2] = "list_possible",
			[3] = _global.options["monitor"],
		}
	})
	return tostring(data.stdout)
end

function current_rate()
	mp.msg.info("Querying Refresh Rate...")
	local data = _global.utils.subprocess({
		["cancellable"] = false,
		["args"] = {
			[1] = _global.options["program"],
			[2] = "current",
			[3] = _global.options["monitor"],
		}
	})
	mp.msg.info(tostring(data.stdout))
	return tonumber(tostring(string.gsub(data.stdout:gsub('%W',''),"CurrentRefreshRate","")))
end

function initial_setup()
	if (_global.rates_actual == "" and _global.options["enabled"] == true and _global.item_count == 0) then
		mp.unobserve_property(initial_setup)
		_global.options["monitor"] = mp.get_property("display-names")
		mp.msg.info("Using monitor -:" .. _global.options["monitor"])	
		_global.rate_initial = current_rate()
		_global.lastDrr = _global.rate_initial
		if (_global.rate_initial > 0) then
			mp.msg.info("Saving Initial Refresh Rate -: " .. tostring(_global.rate_initial))
			function revertDrr()
				if (_global.lastDrr ~= _global.rate_initial) then
					mp.msg.info("Reverting Refresh Rate To -: " .. tostring(_global.rate_initial))
					change_rate(_global.rate_initial)
				end
			end
			mp.register_event("shutdown", revertDrr)
		end				
		_global.rates_actual = possible_rate()
		mp.msg.info("Possible Refresh Rates (Actual) -: " .. _global.rates_actual)
		if(_global.options["rates"] ~= "") then
			mp.msg.info("Preferred Refresh Rates (User) -: " .. _global.options["rates"])
		end
	end
end

function main(name, fps)
	if(_global.options["enabled"] == true) then
		if ((_global.trigger_refreshFound == false) or (_global.options["method"] == "always")) then
			mp.msg.info("New Media Loaded #" .. tostring(_global.item_count) .. ", Estimated Refresh Rate -: " .. tostring(fps) .. ", Type -: " .. name)
			_global.temp["rates_internal"] = rate_builder(fps)
			findRefreshRate(fps)
			_global.trigger_refreshFound = true
		end
	end
end

function findRefreshRate(fps)
	if (fps == nil) then
		mp.msg.info("Current Refresh Rate -: No Switch (No Video FPS Available)")
		return
    elseif (math.floor(fps) == _global.lastDrr) then
		mp.msg.info("Current Refresh Rate -: No Switch (Match)")
		return
	elseif (_global.temp["rates_internal"] == "") then
		mp.msg.info("Current Refresh Rate -: No Switch (No Request)")
        return
	else
		setRate(_global.temp["rates_internal"])
    end
end

function setRate(rate)
	mp.msg.info("Current Refresh Rate -: " .. tostring(_global.lastDrr) .. ", Requested Rates -: " .. tostring(rate))
	local paused = mp.get_property("pause")
	if (_global.options["pause"] > 0 and paused ~= "yes") then
		mp.set_property("pause", "yes")
		paused = mp.get_property("pause")
	end

	local new_rate = change_rate(rate)
	
	if (_global.options["pause"] > 0 and paused == "yes") then
		mp.msg.info("Pausing Playback for " .. tostring(_global.options["pause"]) .. " second(s)")
		sleep(_global.options["pause"])
		mp.set_property("pause", "no")
	end

	if(_global.lastDrr ~= tonumber(new_rate)) then
		if (tonumber(new_rate) == 23) then
			mp.msg.info("Manually Setting Display-FPS Property -: 23.97")
			mp.set_property_number("display-fps", 23.97)
		elseif (tonumber(new_rate) == 59) then
			mp.msg.info("Manually Setting Display-FPS Property -: 59.94")
			mp.set_property_number("display-fps", 59.94)
   		else
			mp.msg.info("Manually Setting Display-FPS Property -: " .. new_rate)
			mp.set_property("display-fps", tonumber(new_rate))
		end		
		
		_global.lastDrr = tonumber(new_rate)
	end
end

function rate_builder(rate)
	local rates_table = {}
	mp.msg.info("Calculating Refresh Rates")
	if (check_rates(_global.rates_actual, tostring(math.floor(rate))) and (math.floor(rate) ~= _global.lastDrr)) then table.insert(rates_table, tostring(math.floor(rate))) end
	if(math.floor(rate) ~= math.ceil(rate)) then
		if (check_rates(_global.rates_actual, tostring(math.ceil(rate))) and (math.ceil(rate) ~= _global.lastDrr)) then table.insert(rates_table, tostring(math.ceil(rate))) end
		if (check_rates(_global.rates_actual, tostring(math.floor(rate) + math.ceil(rate))) and ((math.floor(rate) + math.ceil(rate)) ~= _global.lastDrr)) then table.insert(rates_table, tostring(math.floor(rate) + math.ceil(rate))) end
	end
	for i=10,2,-1 
	do 
		if (check_rates(_global.rates_actual, tostring(math.floor(rate) * i)) and (math.floor(rate) * i ~= _global.lastDrr)) then table.insert(rates_table, tostring(math.floor(rate) * i)) end
		if(math.floor(rate) ~= math.ceil(rate)) then
			if (check_rates(_global.rates_actual, tostring(math.ceil(rate) * i)) and (math.ceil(rate) * i ~= _global.lastDrr)) then table.insert(rates_table, tostring(math.ceil(rate) * i)) end
		end
	end
	mp.msg.info("Calculated Refresh Rates (Script) -: " .. table.concat(rates_table, ";"))
	
	if(_global.options["rates"] ~= "") then
		local rates_table_preferred = {}
		for i, rates_table in ipairs(rates_table) do
			if (check_rates(_global.options["rates"], tostring(rates_table))) then table.insert(rates_table_preferred, rates_table) end
		end
		mp.msg.info("Culled Refresh Rates (User) -: " .. table.concat(rates_table_preferred, ";"))	
		return table.concat(rates_table_preferred, ";")
	end
	
	return table.concat(rates_table, ";")

end

function start()
    mp.unobserve_property(start)
	_global.temp = {}
	if(_global.options["theme"] == true and _global.item_count == 1) then mp.msg.info("Looks like theme video, skipping refresh switch") end
	if (_global.options["enabled"] == true and (_global.options["theme"] == false or (_global.options["theme"] == true and _global.item_count > 1))) then
		mp.msg.info("Refresh Ajustment Script Enabled")
		_global.trigger_refreshFound = false
		if not (_global.rate_initial) then
			return
		end
		local test = mp.get_property("container-fps")
		if (test == nil or test == "nil property unavailable") then
			test = mp.get_property("estimated-vf-fps")
			if (test == nil or test == "nil property unavailable") then
				return
			end
			mp.observe_property("estimated-vf-fps", "number", main)
		else
			mp.observe_property("container-fps", "number", main)
		end
	end 
end

function check()
	initial_setup()
	_global.item_count = _global.item_count + 1
    mp.observe_property("estimated-vf-fps", "string", start)
end

mp.register_event("file-loaded", check)