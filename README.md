# OVMSLogger (Beta)
 
![image](https://user-images.githubusercontent.com/6816385/160421841-d837ac47-5e6f-45fe-be41-7e1f4955f81a.png)

OVMSLogger is a self-hosted logger on Raspberry Pi or Docker. It is based on the famous [Teslalogger](https://github.com/bassmaster187/TeslaLogger) and uses many of its funktionalities.

## What do you need: 
- OVMS Hardware: https://www.openvehicles.com/
- OVMS connected to ovms.dexters-web.de
- Basic Teslalogger installlation: https://github.com/bassmaster187/TeslaLogger
- Location streaming set to 10 Seconds in your OVMS/Config/Notifications:
 ![image](https://user-images.githubusercontent.com/6816385/160799421-fd5763c3-1ede-484a-b0bf-516af9a60389.png)
 
## Tested Cars:
- Tesla Roadster
- Tesla Model S
  - Known Problems: Supercharging is not detected
- Hyundai Ioniq VFL (28kWh) OVMS Firmware 3.3.002 or newer!!!
  - Known Problems: CanBus sometimes is loosing connection to OVMS. We are investigating

Depending on OVMS signals send to OVMSLogger you will have more or less features. For instance: Cell Temperature are not sent by all cars...

Basically all cars [supported by OVMS](https://docs.openvehicles.com/en/latest/introduction.html#vehicle-support) should work if OVMS supports:
- GPS Location
- SOC (detect charging)
- Speed (detect driving)

Please tell us, if you sucesssuflly logged a car or not with OVMSLogger / Teslalogger.

# Connect your OVMS car to Teslalogger
- Go to http://raspberry/admin/ovms.php or http://localhost:8888/admin/ovms.php depending on your installation.
- Enter your ovms.dexters-web.de server credentials and car name.
- Restart Teslalogger

If everything works fine, you can see OVMSLogger starting in Logfile:

```
30.03.2022 11:12:45 : Start OVMSLogger V1.0.5.0
```

If your dexters credentials are working you will see
```
30.03.2022 11:12:49 : #4: Auth Result: Login ok
30.03.2022 11:12:49 : #4: Vehicles: [{"id":"ERNAIONIQ","v_apps_connected":0,"v_btcs_connected":0,"v_net_connected":1}]
30.03.2022 11:12:49 : #4: Car found in account!
```
