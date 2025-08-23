# ChargeControl

This project enables to combine a power plant, like a photovoltaic
system with your e-car charger. 

In my case I own a Fronius Inverter and a go-e Charger. 

Both devices support an simple API to fetch and set values, like 
current power producing, battery level information, current load,
or on charger side the amount of current to load, the maximum power
to load into the car, and more. 

To support also other devices easily the code implements two interfaces:

* IPowerPlant
* ICharger

Which need to implement `UpdateValues` method to fetch the current values.
This is done every few seconds. Based on those values, the interfaces
need to implement a few methods which then are used to start or stop charging,
changing the current current to use.

Depending on the currently produces power, and the current load, the charge-controller
increases or decreases also the current. This should enable an optimal load.

## Fronius API

To allow using the Fronius Solar API which i use with the `FroniusClient`, you need to 
enable the Solar API v1 via HTTP on your inverter first. 

This is possible at:

http://<your-inverter-ip-or-name>/app/solar-api

To ease access, maybe just add a dns name on your router, like "fronius" or similar, 
so you can acces

http://fronius/app/solar-api

Documentation about the Fronius Solar API can be found here: 

https://www.fronius.com/~/downloads/Solar%20Energy/Operating%20Instructions/42,0410,2012.pdf

## Go-E Charger

Go-E charger supports an own http API which is documented here:

https://github.com/goecharger/go-eCharger-API-v2

## Improvements

Software can always be improved,.. so also this little side-project of myself. Some
open things are listed here:

* Currently, the Base URLs are hard-coded, make it configurable
* There is almost no error handling
* There are no tests yet
* Overproduction limits are hardcoded (+/- 1 kWh)
* Interfaces lacks documentation

and most likely many others.

## Commercial use

This project is NOT meant to be commercially used or sold to someone. I publish it
under GPL v3, 

## Contributing

If someone wants to extend the code, add new chargers or other power plant
sources, just fork and open a PR. I am open to everything. 
