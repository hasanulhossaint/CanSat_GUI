# CanSat_GUI
# Project Title

## Required NuGet Packages
Install these in Visual Studio:
- `ScottPlot.WinForms` (for real-time graphs)
- `System.IO.Ports` (for Arduino communication)

## Arduino Data Format Example
Your Arduino should send data as CSV in this order: lat,lon,alt_gps,speed,satellites,ax,ay,az,pitch,roll,yaw,temp_bmp,pressure,alt_bmp,temp_dht,humidity,battery_voltage,battery_percent,regulator_voltage,co,co2,ch4,free_fall,sd_status
## Example data line:
12.3456,98.7654,150.0,5.2,8,0.12,0.98,9.81,1.2,3.4,5.6,25.3,1013.25,148.7,24.8,45.6,3.7,80,3.3,10,400,20,0,Logging


## Key Features
- **Real-Time Plots:**
  - Acceleration (X/Y/Z)
  - Gyroscope (Pitch/Roll/Yaw)
  
- **Sensor Data Display:**
  - GPS coordinates, altitude, speed
  - Temperature (BMP180 + DHT22)
  - Humidity, pressure
  - Battery status
  - Gas sensor readings
  
- **System Monitoring:**
  - Free-fall detection
  - SD card status
  - Voltage regulator output

## How to Use
1. **Install NuGet Packages:** `ScottPlot.WinForms` + `System.IO.Ports`
2. **Set Correct COM Port:** Change "COM3" in `InitializeSerialPort()` to match your Arduino
3. **Upload Arduino Code:** Ensure your Arduino sends data in the specified CSV format
4. **Run the Application:** Data will update automatically when received
