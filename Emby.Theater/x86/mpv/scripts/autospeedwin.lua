--[[
    See script details on https://github.com/kevinlekiller/mpv_scripts
    
    Valid --script-opts are (they are all optional):
    autospeed-enabled=false      true/false - Use nircmd to change the refresh rate of your monitor.
    autospeed-speed=false       true/false - Adjust speed of the video?.
    autospeed-nircmdc="nircmdc" String     - Path to nircmdc executable file. If not set, nircmdc will be searched in Windows PATH variable.
    autospeed-monitor=0         Number     - Which monitor (display) to set the refresh rate on.
    autospeed-dwidth=1920       Number     - Display width.
    autospeed-dheight=1080      Number     - Display height.
    autospeed-bdepth=32         Number     - Display bit depth.
    autospeed-rates="60"        String     - String of refresh rates your monitor supports and you want to use, separated by semicolons.
                                             Nircmd seems to prefer rounded numbers, 72 instead of 71.92 for example.
                                             Examples: autospeed-rates="60" | autospeed-rates="50;60;72"
                                             Note if you want a rate to be prefered over another, for example the video is 24hz
                                             and your rates are "100;120;144", but you want 144hz instead of 120hz for 24fps videos, change the
                                             order to autospeed-rates="100;144;120", then 144hz will take precendence over 120hz.
    autospeed-exitrate=60       Number     - Which refresh rate to set when exiting mpv. Set to 0 to ignore.
    autospeed-minspeed=0.9      Number     - Minimum allowable speed to play video at.
    autospeed-maxspeed=1.1      Number     - Maximum allowable speed to play video at.
    autospeed-osd=true          true/false - Enable OSD.
    autospeed-osdtime=10        Number     - How many seconds the OSD will be shown.
    autospeed-osdkey=y                     - Key to press to show the OSD.
    autospeed-estfps=false      true/false - Calculate/change speed if a video has a variable fps at the cost of higher CPU usage (most videos have a fixed fps).
    autospeed-spause            true/false - Pause video while switching display modes.
                                Number     - If you set this a number, it will pause for that amount of seconds.
    autospeed-method="always"   String     - Set how often to check if framerate conforms to settings
					     "always": (original behaviour) contstantly checks during playback
					     "start": only does initial change at start
						 
    Example: mpv file.mkv --script-opts=autospeed-enabled=true,autospeed-minspeed=0.8
--]]
--[[
    Copyright (C) 2016-2017  kevinlekiller

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
    https://www.gnu.org/licenses/gpl-2.0.html
--]]
local _global = {
    osd_start = mp.get_property_osd("osd-ass-cc/0"),
    osd_end = mp.get_property_osd("osd-ass-cc/1"),
    utils = require 'mp.utils',
    rateCache = {},
    lastDrr = 0,
    speedCache = {},
    next = next,
    trigger_refreshFound = false,
	initial_start = false,
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

function osdEcho()
    if (_global.options["osd"] ~= true) then
        return
    end
    setOSD()
    if (_global.temp["output"] ~= nil) then
        mp.osd_message(_global.temp["output"], _global.options["osdtime"])
    end
end

function getOptions()
    _global.options = {
        ["enabled"]    = false,
        ["speed"]     = false,
        ["program"]   = "refreshrate",
        ["monitor"]   = 0,
        ["rates"]     = "",
		["exitrate"]     = 0,
        ["minspeed"]  = 0.9,
        ["maxspeed"]  = 1.1,
        ["osd"]       = false,
        ["osdtime"]   = 10,
        ["osdkey"]    = "y",
        ["estfps"]    = false,
		["skipmultiples"]    = false,
        ["spause"]    = 3,
		["method"]	  = "once",
		["theme"] = false,
    }
    for key, value in pairs(_global.options) do
        local opt = mp.get_opt("autospeed-" .. key)
        if (opt ~= nil) then
            if ((key == "enabled" or key == "speed" or key == "skipmultiples" or key == "osd" or key == "estfps" or key == "theme") and opt == "true") then
                _global.options[key] = true
            elseif (key == "minspeed" or key == "maxspeed" or key == "osdtime" --[[or key == "monitor"--]] or key == "spause") then
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

function main(name, fps)
	if(_global.options["enabled"] == true) then
		if ((_global.trigger_refreshFound == false) or (_global.options["method"] == "always")) then
			mp.msg.info("New Media Loaded #" .. tostring(_global.item_count) .. ", Estimated Refresh Rate -: " .. tostring(fps) .. ", Type -: " .. name)
			_global.temp["rates_internal"] = rate_builder(fps)
			mp.msg.info("Try Speed Adjustment -: " .. tostring(_global.options["speed"]))
			if (fps == nil) then
				return
			end
			_global.temp["fps"] = fps
			findRefreshRate(fps)
			determineSpeed()
			if (_global.options["speed"] == true and _global.temp["speed"] >= _global.options["minspeed"] and _global.temp["speed"] <= _global.options["maxspeed"]) then
				mp.set_property_number("speed", _global.temp["speed"])
				mp.msg.info("Adjusting Speed -: " .. tostring(_global.temp["speed"]))
			else
				_global.temp["speed"] = _global.confSpeed
			end
			_global.trigger_refreshFound = true
		end
	end
end

function setOSD()
    _global.temp["output"] = (_global.osd_start ..
        "{\\b1}Original monitor refresh rate{\\b0}\\h\\h" .. _global.temp["initial_drr"] .. "Hz\\N" ..
        "{\\b1}Current  monitor refresh rate{\\b0}\\h\\h" .. _global.temp["drr"] .. "Hz\\N" ..
        "{\\b1}Original video fps{\\b0}\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h" .. _global.temp["fps"] .. "fps\\N" ..
        "{\\b1}Current  video fps{\\b0}\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h" .. (_global.temp["fps"] * _global.temp["speed"]) .. "fps\\N" ..
        "{\\b1}Original mpv speed setting{\\b0}\\h\\h\\h\\h\\h\\h" .. _global.confSpeed .. "x\\N" ..
        "{\\b1}Current  mpv speed setting{\\b0}\\h\\h\\h\\h\\h\\h" .. _global.temp["speed"] .. "x" ..
        _global.osd_end
    )
end

function determineSpeed()
    local id = _global.temp["drr"] .. _global.temp["fps"]
    if (_global.speedCache[id] ~= nil) then
        _global.temp["speed"] = _global.speedCache[id]
        return
    end
    if (_global.temp["drr"] > _global.temp["fps"]) then
        local difference = (_global.temp["drr"] / _global.temp["fps"])
        if (difference >= 2) then
            -- fps = 24fps, drr = 60hz
            -- difference = 60hz/24fps = 3 rounded
            -- 24fps * 3 = 72fps
            -- 60hz / 72fps = 0.833333333333 speed
            -- 72fps * 0.833333333333 = 60fps
            _global.temp["speed"] = (_global.temp["drr"] / (_global.temp["fps"] * round(difference)))
        else
            -- fps = 50fps, drr = 60hz
            -- 60hz / 50fps = 1.2 speed
            -- 50fps * 1.2 speed = 60fps
            
            -- fps = 59.94fps, drr = 60hz
            -- 60hz / 59.94fps  = 1.001001001001001 speed
            -- 59.94fps * 1.001001001001001 = 60fps
            _global.temp["speed"] = difference
        end
    elseif (_global.temp["drr"] < _global.temp["fps"]) then
        local difference = (_global.temp["fps"] / _global.temp["drr"])
        if (difference >= 2) then
            -- fps = 120fps, drr = 25hz
            -- difference = 120fps/25hz = 5 rounded
            -- 120fps/5 = 24fps ; 25hz / 24fps = 1.04166666667 speed
            -- 24fps * 1.04166666667 speed = 25fps
            _global.temp["speed"] = (_global.temp["drr"] / (_global.temp["fps"] / round(difference)))
        else
            -- fps = 60fps, drr = 50hz
            -- difference = 50hz / 60fps = 0.833333333333 speed
            -- 60fps * 0.833333333333 speed = 50fps
            
            -- fps = 60fps, drr = 59.94hz
            -- difference = 59.94hz / 60fps = 0.999 speed
            -- 60fps * 0.999 speed = 59.94fps
            _global.temp["speed"] = (_global.temp["drr"] / _global.temp["fps"])
        end
    elseif (_global.temp["drr"] == _global.temp["fps"]) then
        _global.temp["speed"] = 1
    end
    _global.speedCache[id] = _global.temp["speed"]
end

function findRefreshRate(fps)
    -- This is to prevent a system call if the screen refresh / video fps has not changed.
    if (math.floor(fps) == _global.lastDrr) then
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
	mp.msg.info("Current Refresh Rate -: " .. tostring(_global.temp["initial_drr"]) .. ", Requested Rates -: " .. tostring(rate))
	local paused = mp.get_property("pause")
	if (_global.options["spause"] > 0 and paused ~= "yes") then
		mp.set_property("pause", "yes")
		paused = mp.get_property("pause")
	end

	local new_rate = change_rate(rate)
	
	if (_global.options["spause"] > 0 and paused == "yes") then
		mp.msg.info("Pausing Playback for " .. tostring(_global.options["spause"]) .. " second(s)")
		sleep(_global.options["spause"])
		mp.set_property("pause", "no")
	end

    _global.temp["drr"] = tonumber(new_rate)
    _global.lastDrr = _global.temp["drr"]
end

function rate_builder(rate)
	local actual_rates = possible_rate()
	mp.msg.info("Possible Refresh Rates (Actual) -: " .. actual_rates)
	local rates_table = {}
	if(_global.options["rates"] == "") then
		if check_rates(actual_rates, tostring(math.floor(rate))) then table.insert(rates_table, tostring(math.floor(rate))) end
		if check_rates(actual_rates, tostring(math.ceil(rate))) then table.insert(rates_table, tostring(math.ceil(rate))) end
		if check_rates(actual_rates, tostring(math.floor(rate) + math.ceil(rate))) then table.insert(rates_table, tostring(math.floor(rate) + math.ceil(rate))) end
		if(_global.options["skipmultiples"] == false) then
			for i=10,2,-1 
			do 
				if check_rates(actual_rates, tostring(math.floor(rate) * i)) then table.insert(rates_table, tostring(math.floor(rate) * i)) end
				if check_rates(actual_rates, tostring(math.ceil(rate) * i)) then table.insert(rates_table, tostring(math.ceil(rate) * i)) end
			end
		end
		mp.msg.info("Possible Refresh Rates (Computed) -: " .. table.concat(rates_table, ";"))
		return table.concat(rates_table, ";")
	else
		mp.msg.info("Provided Refresh Rates (User) -: " .. _global.options["rates"])
		for x in string.gmatch(_global.options["rates"],'([^;]+)')
		do
			if check_rates(actual_rates, tostring(math.floor(x))) then table.insert(rates_table, tostring(math.floor(x))) end
		end
		mp.msg.info("Possible Refresh Rates (User) -: " .. table.concat(rates_table, ";"))
		return table.concat(rates_table, ";")
	end

end

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
	mp.msg.info("Querying Supported Refresh Rates...")
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

function start()
    mp.unobserve_property(start)
	_global.options["monitor"] = mp.get_property("display-names")
	mp.msg.info(_global.options["monitor"])	
	_global.temp = {}
	_global.item_count = _global.item_count + 1
	_global.temp["initial_drr"] = current_rate()
	if(_global.options["theme"] == true and _global.item_count == 1) then mp.msg.info("Looks like theme video, skipping refresh switch") end
	if (_global.options["enabled"] == true and (_global.options["theme"] == false or (_global.options["theme"] == true and _global.item_count > 1))) then
		mp.msg.info("Refresh Ajustment Script Enabled")
		_global.trigger_refreshFound = false
		_global.rateCache = {}
		_global.speedCache = {}
		if(_global.initial_start == false) then
			_global.options["exitrate"] = _global.temp["initial_drr"]
			_global.initial_start = true
			mp.msg.info("Saving Exit Refresh Rate -: " .. tostring(_global.options["exitrate"]))
		end
		if not (_global.temp["initial_drr"]) then
			return
		end
		_global.temp["drr"] = _global.temp["initial_drr"]
		if not (_global.confSpeed) then
			_global.confSpeed = mp.get_property_native("speed")
		end
		local test = mp.get_property("container-fps")
		if (test == nil or test == "nil property unavailable") then
			if (_global.options["estfps"] ~= true) then
				return
			end
			test = mp.get_property("estimated-vf-fps")
			if (test == nil or test == "nil property unavailable") then
				return
			end
			mp.observe_property("estimated-vf-fps", "number", main)
		else
			mp.observe_property("container-fps", "number", main)
		end
		mp.add_key_binding(_global.options["osdkey"], mp.get_script_name(), osdEcho, {repeatable=true})
		if (_global.options["enabled"] == true and _global.options["exitrate"] > 0) then
			function revertDrr()
				mp.msg.info("Reverting Refresh To -: " .. tostring(_global.options["exitrate"]))
				change_rate(_global.options["exitrate"])
			end
			if(_global.item_count == 1) then 
				mp.register_event("shutdown", revertDrr)
			end
		end
	end 
end

-- Wait until we get a video fps.
function check()
    mp.observe_property("estimated-vf-fps", "string", start)
end

mp.register_event("file-loaded", check)
