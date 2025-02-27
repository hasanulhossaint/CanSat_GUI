using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using ScottPlot;

namespace CanSatDashboard
{
    public partial class MainForm : Form
    {
        // ==================== UI Components ====================
        // GPS
        private Label lblLatitude, lblLongitude, lblAltitudeGPS, lblSpeed, lblSatellites;

        // System Status
        private Label lblBatteryVoltage, lblBatteryPercent, lblVoltageRegulator, lblSDStatus, lblFreeFall;

        // Environment
        private Label lblTemperatureBMP, lblTemperatureDHT, lblHumidity, lblPressure;

        // Gas Sensors
        private Label lblCO, lblCO2, lblCH4;

        // Plots
        private FormsPlot accelPlot, gyroPlot, altitudePlot, tempPlot;

        // Circular buffers for plotting
        private double[] accelX = new double[100], accelY = new double[100], accelZ = new double[100];
        private double[] gyroPitch = new double[100], gyroRoll = new double[100], gyroYaw = new double[100];
        private int dataIndex = 0;

        // Serial Port
        private SerialPort serialPort;

        public MainForm()
        {
            InitializeComponent();
            BuildUI();
            InitializeSerialPort("COM3", 9600);
        }

        // ==================== UI Initialization ====================
        private void BuildUI()
        {
            this.Text = "CanSat Dashboard";
            this.Size = new Size(1400, 900);
            this.Font = new Font("Arial", 10);

            // -------------------- GPS Panel --------------------
            Panel gpsPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(300, 120),
                BorderStyle = BorderStyle.FixedSingle
            };
            gpsPanel.Controls.Add(new Label { Text = "GPS Data", Location = new Point(10, 5), Font = new Font(this.Font, FontStyle.Bold) });
            lblLatitude = CreateLabel(gpsPanel, 30, "Latitude: -");
            lblLongitude = CreateLabel(gpsPanel, 50, "Longitude: -");
            lblAltitudeGPS = CreateLabel(gpsPanel, 70, "Altitude (GPS): -");
            lblSpeed = CreateLabel(gpsPanel, 90, "Speed: - m/s");

            // -------------------- System Status Panel --------------------
            Panel statusPanel = new Panel
            {
                Location = new Point(340, 20),
                Size = new Size(300, 120),
                BorderStyle = BorderStyle.FixedSingle
            };
            statusPanel.Controls.Add(new Label { Text = "System Status", Location = new Point(10, 5), Font = new Font(this.Font, FontStyle.Bold) });
            lblBatteryVoltage = CreateLabel(statusPanel, 30, "Battery: - V");
            lblBatteryPercent = CreateLabel(statusPanel, 50, "Charge: - %");
            lblVoltageRegulator = CreateLabel(statusPanel, 70, "Regulator: - V");
            lblSDStatus = CreateLabel(statusPanel, 90, "SD Card: Idle");

            // -------------------- Environment Panel --------------------
            Panel envPanel = new Panel
            {
                Location = new Point(660, 20),
                Size = new Size(300, 120),
                BorderStyle = BorderStyle.FixedSingle
            };
            envPanel.Controls.Add(new Label { Text = "Environment", Location = new Point(10, 5), Font = new Font(this.Font, FontStyle.Bold) });
            lblTemperatureBMP = CreateLabel(envPanel, 30, "Temp (BMP): - °C");
            lblTemperatureDHT = CreateLabel(envPanel, 50, "Temp (DHT): - °C");
            lblHumidity = CreateLabel(envPanel, 70, "Humidity: - %");
            lblPressure = CreateLabel(envPanel, 90, "Pressure: - hPa");

            // -------------------- Gas Sensors Panel --------------------
            Panel gasPanel = new Panel
            {
                Location = new Point(980, 20),
                Size = new Size(300, 120),
                BorderStyle = BorderStyle.FixedSingle
            };
            gasPanel.Controls.Add(new Label { Text = "Gas Sensors", Location = new Point(10, 5), Font = new Font(this.Font, FontStyle.Bold) });
            lblCO = CreateLabel(gasPanel, 30, "CO: - ppm");
            lblCO2 = CreateLabel(gasPanel, 50, "CO₂: - ppm");
            lblCH4 = CreateLabel(gasPanel, 70, "CH₄: - ppm");
            lblFreeFall = CreateLabel(gasPanel, 90, "Free Fall: No");

            // -------------------- Plots --------------------
            accelPlot = CreatePlot("Acceleration (m/s²)", new Point(20, 160), 600, 200);
            gyroPlot = CreatePlot("Gyroscope (°/s)", new Point(20, 380), 600, 200);
            altitudePlot = CreatePlot("Altitude (m)", new Point(660, 160), 600, 200);
            tempPlot = CreatePlot("Temperature (°C)", new Point(660, 380), 600, 200);

            this.Controls.AddRange(new Control[] { gpsPanel, statusPanel, envPanel, gasPanel });
        }

        // ==================== Helper Methods ====================
        private Label CreateLabel(Panel parent, int y, string text)
        {
            Label lbl = new Label
            {
                Location = new Point(10, y),
                Text = text,
                AutoSize = true
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private FormsPlot CreatePlot(string title, Point location, int width, int height)
        {
            FormsPlot plot = new FormsPlot
            {
                Location = location,
                Size = new Size(width, height)
            };
            plot.Plot.Title(title);
            plot.Plot.XLabel("Time");
            plot.Plot.YLabel(title.Split(' ')[0]);
            this.Controls.Add(plot);
            return plot;
        }

        // ==================== Serial Communication ====================
        private void InitializeSerialPort(string portName, int baudRate)
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.DataReceived += SerialPort_DataReceived;
            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string rawData = serialPort.ReadLine().Trim();
            string[] values = rawData.Split(',');

            if (values.Length >= 15) // Adjust based on your data format
            {
                Invoke((MethodInvoker)delegate
                {
                    // -------------------- Parse Data --------------------
                    // Example format: 
                    // lat,lon,alt_gps,speed,satellites,
                    // ax,ay,az,pitch,roll,yaw,
                    // temp_bmp,pressure,alt_bmp,
                    // temp_dht,humidity,
                    // battery_voltage,battery_percent,regulator_voltage,
                    // co,co2,ch4,free_fall,sd_status

                    // GPS
                    UpdateLabel(lblLatitude, $"Latitude: {values[0]}");
                    UpdateLabel(lblLongitude, $"Longitude: {values[1]}");
                    UpdateLabel(lblAltitudeGPS, $"Altitude: {values[2]} m");
                    UpdateLabel(lblSpeed, $"Speed: {values[3]} m/s");

                    // IMU
                    UpdateAcceleration(float.Parse(values[5]), float.Parse(values[6]), float.Parse(values[7]));
                    UpdateGyro(float.Parse(values[8]), float.Parse(values[9]), float.Parse(values[10]));

                    // Environment
                    UpdateLabel(lblTemperatureBMP, $"Temp (BMP): {values[11]} °C");
                    UpdateLabel(lblPressure, $"Pressure: {values[12]} hPa");
                    UpdateLabel(lblTemperatureDHT, $"Temp (DHT): {values[13]} °C");
                    UpdateLabel(lblHumidity, $"Humidity: {values[14]} %");

                    // Power
                    UpdateLabel(lblBatteryVoltage, $"Battery: {values[15]} V");
                    UpdateLabel(lblBatteryPercent, $"Charge: {values[16]} %");
                    UpdateLabel(lblVoltageRegulator, $"Regulator: {values[17]} V");

                    // Gas Sensors
                    UpdateLabel(lblCO, $"CO: {values[18]} ppm");
                    UpdateLabel(lblCO2, $"CO₂: {values[19]} ppm");
                    UpdateLabel(lblCH4, $"CH₄: {values[20]} ppm");

                    // Status
                    UpdateLabel(lblFreeFall, $"Free Fall: {(values[21] == "1" ? "Yes" : "No")}");
                    UpdateLabel(lblSDStatus, $"SD Card: {values[22]}");
                });
            }
        }

        // ==================== Data Update Methods ====================
        private void UpdateLabel(Label label, string text)
        {
            label.Text = text;
        }

        private void UpdateAcceleration(float ax, float ay, float az)
        {
            UpdateBuffer(accelX, ax);
            UpdateBuffer(accelY, ay);
            UpdateBuffer(accelZ, az);

            accelPlot.Plot.Clear();
            accelPlot.Plot.AddSignal(accelX, color: Color.Cyan, label: "X");
            accelPlot.Plot.AddSignal(accelY, color: Color.LimeGreen, label: "Y");
            accelPlot.Plot.AddSignal(accelZ, color: Color.OrangeRed, label: "Z");
            accelPlot.Plot.Legend();
            accelPlot.Render();
        }

        private void UpdateGyro(float pitch, float roll, float yaw)
        {
            UpdateBuffer(gyroPitch, pitch);
            UpdateBuffer(gyroRoll, roll);
            UpdateBuffer(gyroYaw, yaw);

            gyroPlot.Plot.Clear();
            gyroPlot.Plot.AddSignal(gyroPitch, color: Color.Cyan, label: "Pitch");
            gyroPlot.Plot.AddSignal(gyroRoll, color: Color.LimeGreen, label: "Roll");
            gyroPlot.Plot.AddSignal(gyroYaw, color: Color.OrangeRed, label: "Yaw");
            gyroPlot.Plot.Legend();
            gyroPlot.Render();
        }

        private void UpdateBuffer(double[] buffer, double newValue)
        {
            Array.Copy(buffer, 1, buffer, 0, buffer.Length - 1);
            buffer[^1] = newValue;
        }
    }
}
