using ScottPlot.Plottables;
using ScottPlot;
using System.IO.Ports;
using System.Management;
using System;
using System.Drawing;
using static ScottPlot.Color;
using static OpenTK.Graphics.OpenGL.GL;
using ScottPlot.WinForms;
using static System.Windows.Forms.DataFormats;

namespace ESPscope2
{
    struct powerDataParama
    {
        public double start_time;
        public double sample_rate;
    };
    struct powerData
    {
        public double[] current;
        public double[] voltage;
        public double[] current_sum;
        public double[] voltage_sum;
    };
    struct DisData
    {
        public double sample_factor; //every n data add to guide display;
        public int data_size;
        public powerData data;
    };
    struct HorizontalTextSpan
    {
        public HorizontalSpan span;
        public Text text;
    };

    public partial class ESPScope : Form
    {
        const uint max_buffer_size = 200 * 1000; //200 * 1000 *1000
        uint cur_recv_size = 0;
        powerData buffer;
        HorizontalSpan guideSpan;
        DisData guideDisData;
        DisData mainDisData;
        SerialPort serialPort;
        bool is_auto_follow = false;
        int auto_follow_windows = 0;
        AxisSpanUnderMouse? SpanBeingDragged = null;
        AxisLine? PlottableBeingDragged = null;
        AxisSpanUnderMouse? PlotMainSpanBeingDragged = null;
        List<VerticalLine> mainverticalLines;
        List<HorizontalTextSpan> horizontalTextSpans;
        powerDataParama power_data_parama;
        Signal sigGuideCurr, sigGuideVolt;
        Signal sigMainCurr, sigMainVolt;
        public ESPScope()
        {
            buffer = new powerData();
            mainverticalLines = new List<VerticalLine>();
            horizontalTextSpans = new List<HorizontalTextSpan>();
            buffer.current = new double[max_buffer_size];
            buffer.voltage = new double[max_buffer_size];
            buffer.current_sum = new double[max_buffer_size];
            buffer.voltage_sum = new double[max_buffer_size];
            power_data_parama = new powerDataParama();
            power_data_parama.sample_rate = 3012.048;
            InitializeComponent();
            InitializeDeviceWatcher();
            InitializeSerialPorts();
            InitializePlot();
            panelControl.BackColor = System.Drawing.Color.FromArgb(0x33, 0x33, 0x33);
        }

        private void InitializePlot()
        {
            //guide plot init
            guideDisData = new DisData();
            guideDisData.sample_factor = 1;
            guideDisData.data_size = 10000;
            guideDisData.data.current = new double[guideDisData.data_size];
            guideDisData.data.voltage = new double[guideDisData.data_size];


            sigGuideCurr = formsPlotGuide.Plot.Add.Signal(guideDisData.data.current);
            sigGuideVolt = formsPlotGuide.Plot.Add.Signal(guideDisData.data.voltage);
            sigGuideCurr.Data.Period = guideDisData.sample_factor / power_data_parama.sample_rate;
            sigGuideVolt.Data.Period = guideDisData.sample_factor / power_data_parama.sample_rate;

            guideSpan = formsPlotGuide.Plot.Add.HorizontalSpan(0, guideDisData.data_size * guideDisData.sample_factor / power_data_parama.sample_rate);
            guideSpan.IsDraggable = true;
            guideSpan.IsResizable = true;

            formsPlotGuide.MouseDown += formsPlotGuide_MouseDown;
            formsPlotGuide.MouseUp += formsPlotGuide_MouseUp;
            formsPlotGuide.MouseMove += formsPlotGuide_MouseMove;

            //main plot init
            mainDisData = new DisData();
            mainDisData.sample_factor = 1;
            mainDisData.data_size = 100000;
            mainDisData.data.current = new double[mainDisData.data_size];
            mainDisData.data.voltage = new double[mainDisData.data_size];

            sigMainCurr = formsPlotMain.Plot.Add.Signal(mainDisData.data.current);
            sigMainVolt = formsPlotMain.Plot.Add.Signal(mainDisData.data.voltage);
            sigMainCurr.Axes.YAxis = formsPlotMain.Plot.Axes.Left;
            sigMainVolt.Axes.YAxis = formsPlotMain.Plot.Axes.Right;

            sigMainCurr.Data.Period = guideDisData.sample_factor / power_data_parama.sample_rate;
            sigMainVolt.Data.Period = guideDisData.sample_factor / power_data_parama.sample_rate;
            formsPlotMain.Plot.Axes.Left.Label.Text = "Current (mA)";
            formsPlotMain.Plot.Axes.Right.Label.Text = "Voltage (V)";

            //formsPlotMain.Plot.Axes.Left.Label.BackColor = sig1.Color;
            //formsPlotMain.Plot.Axes.Right.Label.BackColor = sig2.Color;
            formsPlotMain.MouseDown += FormsPlotMain_MouseDown;
            formsPlotMain.MouseUp += FormsPlotMain_MouseUp;
            formsPlotMain.MouseMove += FormsPlotMain_MouseMove;


            formsPlotMain.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#333333");
            formsPlotMain.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#333333");
            formsPlotMain.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#0b3049");
            formsPlotMain.Plot.Axes.Color(ScottPlot.Color.FromHex("#a0acb5"));

            formsPlotGuide.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#333333");
            formsPlotGuide.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#333333");
            formsPlotGuide.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#0b3049");
            formsPlotGuide.Plot.Axes.Color(ScottPlot.Color.FromHex("#a0acb5"));

            formsPlotMain.Plot.Axes.Left.Label.ForeColor = sigMainCurr.Color;
            formsPlotMain.Plot.Axes.Right.Label.ForeColor = sigMainVolt.Color;
            // plot sample DateTime data

        }



        private void formsPlotGuide_MouseDown(object? sender, MouseEventArgs e)
        {

            var thingUnderMouse = GetSpanUnderMouse(e.X, e.Y);
            if (thingUnderMouse is not null)
            {
                SpanBeingDragged = thingUnderMouse;
                formsPlotGuide.Interaction.Disable(); // disable panning while dragging
            }

        }

        private void formsPlotGuide_MouseUp(object? sender, MouseEventArgs e)
        {
            SpanBeingDragged = null;
            formsPlotGuide.Interaction.Enable(); // enable panning
            formsPlotGuide.Refresh();


        }
        private void reselect_mainplot_datarange(int startIndex, int endIndex)
        {
            Console.WriteLine("start end£º" + startIndex + " " + endIndex);
            Console.WriteLine("offset: " + startIndex / power_data_parama.sample_rate);
            int len = endIndex - startIndex;
            if (len <= 0)
            {
                return;
            }

            mainDisData.sample_factor = (double)len / mainDisData.data_size;

            uint buffer_index = 0;
            int buffer_index_start = (int)(startIndex % max_buffer_size);
            int i = 0;
            while (i < mainDisData.data_size)
            {
                int index = (int)((i * mainDisData.sample_factor) + buffer_index_start);
                if (index >= max_buffer_size - 1)
                    break;
                mainDisData.data.current[i] = buffer.current[index];
                mainDisData.data.voltage[i] = buffer.voltage[index];
                i++;
            }
            while (i < mainDisData.data_size)
            {
                int index = (int)(((i * mainDisData.sample_factor) + buffer_index_start) % max_buffer_size);
                mainDisData.data.current[i] = buffer.current[index];
                mainDisData.data.voltage[i] = buffer.voltage[index];
                i++;
            }

            sigMainCurr.Data.Period = mainDisData.sample_factor / power_data_parama.sample_rate;
            sigMainVolt.Data.Period = mainDisData.sample_factor / power_data_parama.sample_rate;
            sigMainCurr.Data.XOffset = startIndex / power_data_parama.sample_rate;
            sigMainVolt.Data.XOffset = startIndex / power_data_parama.sample_rate;
            formsPlotMain.Plot.Axes.AutoScale();

            //formsPlotMain.Plot.Axes.AutoScaleX();

            formsPlotMain.Refresh();
        }
        private void formsPlotGuide_MouseMove(object? sender, MouseEventArgs e)
        {
            if (SpanBeingDragged is not null)
            {
                if (SpanBeingDragged.Span is HorizontalSpan)
                {
                    HorizontalSpan horizontalSpan = (HorizontalSpan)SpanBeingDragged.Span;
                    double start = horizontalSpan.X1 < horizontalSpan.X2 ? horizontalSpan.X1 : horizontalSpan.X2;
                    double end = horizontalSpan.X1 > horizontalSpan.X2 ? horizontalSpan.X1 : horizontalSpan.X2;
                    int startIndex = (int)(start * power_data_parama.sample_rate);
                    int endIndex = (int)(end * power_data_parama.sample_rate);

                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                    if (endIndex > (int)cur_recv_size - 1)
                    {
                        endIndex = (int)cur_recv_size - 1;
                    }
                    auto_follow_windows = endIndex - startIndex;
                    reselect_mainplot_datarange(startIndex, endIndex);

                }

                // currently dragging something so update it
                Coordinates mouseNow = formsPlotGuide.Plot.GetCoordinates(e.X, e.Y);
                SpanBeingDragged.DragTo(mouseNow);
                formsPlotGuide.Refresh();
            }
            else
            {
                // not dragging anything so just set the cursor based on what's under the mouse
                var spanUnderMouse = GetSpanUnderMouse(e.X, e.Y);
                if (spanUnderMouse is null) Cursor = Cursors.Default;
                else if (spanUnderMouse.IsResizingHorizontally) Cursor = Cursors.SizeWE;
                else if (spanUnderMouse.IsResizingVertically) Cursor = Cursors.SizeNS;
                else if (spanUnderMouse.IsMoving) Cursor = Cursors.SizeAll;
            }


        }
        private void FormsPlotMain_MouseDown(object? sender, MouseEventArgs e)
        {
            var lineUnderMouse = FormsPlotMainGetLineUnderMouse(e.X, e.Y);
            if (lineUnderMouse is not null)
            {
                PlottableBeingDragged = lineUnderMouse;
                formsPlotMain.Interaction.Disable(); // disable panning while dragging
            }
            var thingUnderMouse = FormsPlotMainGetSpanUnderMouse(e.X, e.Y);
            if (thingUnderMouse is not null)
            {
                PlotMainSpanBeingDragged = thingUnderMouse;
                formsPlotMain.Interaction.Disable(); // disable panning while dragging
            }

        }

        private void FormsPlotMain_MouseUp(object? sender, MouseEventArgs e)
        {
            PlottableBeingDragged = null;
            PlotMainSpanBeingDragged = null;
            formsPlotMain.Interaction.Enable(); // enable panning again
            formsPlotMain.Refresh();
        }
        static string FormatCurrent(double current)
        {
            string unit;
            double value;

            if (Math.Abs(current) >= 1)
            {
                unit = " A";
                value = current;
            }
            else if (Math.Abs(current) >= 1e-3)
            {
                unit = " mA";
                value = current * 1e3;
            }
            else if (Math.Abs(current) >= 1e-6)
            {
                unit = " ¦ÌA";
                value = current * 1e6;
            }
            else
            {
                unit = " nA";
                value = current * 1e9;
            }

            return value.ToString("0.000") + unit;
        }
        static string FormatVoltage(double voltage)
        {
            string unit;
            double value;

            if (Math.Abs(voltage) >= 1)
            {
                unit = " V";
                value = voltage;
            }
            else if (Math.Abs(voltage) >= 1e-3)
            {
                unit = " mV";
                value = voltage * 1e3;
            }
            else if (Math.Abs(voltage) >= 1e-6)
            {
                unit = " ¦ÌV";
                value = voltage * 1e6;
            }
            else
            {
                unit = " nV";
                value = voltage * 1e9;
            }

            return value.ToString("0.000") + unit;
        }

        private void FormsPlotMain_MouseMove(object? sender, MouseEventArgs e)
        {
            // this rectangle is the area around the mouse in coordinate units
            CoordinateRect rect = formsPlotMain.Plot.GetCoordinateRect(e.X, e.Y, radius: 10);

            if (PlottableBeingDragged is null)
            {
                // set cursor based on what's beneath the plottable
                var lineUnderMouse = FormsPlotMainGetLineUnderMouse(e.X, e.Y);
                if (lineUnderMouse is null) Cursor = Cursors.Default;
                else if (lineUnderMouse.IsDraggable && lineUnderMouse is VerticalLine) Cursor = Cursors.SizeWE;
                else if (lineUnderMouse.IsDraggable && lineUnderMouse is HorizontalLine) Cursor = Cursors.SizeNS;
            }
            else
            {
                // update the position of the plottable being dragged
                if (PlottableBeingDragged is HorizontalLine hl)
                {
                    hl.Y = rect.VerticalCenter;
                    hl.Text = $"{hl.Y:0.00}";
                }
                else if (PlottableBeingDragged is VerticalLine vl)
                {
                    vl.X = rect.HorizontalCenter;
                    int index = (int)((vl.X - sigMainCurr.Data.XOffset) / sigMainCurr.Data.Period);
                    if (index < 0)
                    {
                        index = 0;
                    }
                    else if (index > mainDisData.data_size - 1)
                    {
                        index = mainDisData.data_size - 1;
                    }
                    double curr = sigMainCurr.Data.GetY(index);
                    double volt = sigMainVolt.Data.GetY(index);
                    vl.Text = $"T:{vl.X:0.00} C:{curr:0.00} V:{volt:0.00}";
                }
                formsPlotMain.Refresh();
            }

            if (PlotMainSpanBeingDragged is not null)
            {
                if (PlotMainSpanBeingDragged.Span is HorizontalSpan)
                {
                    HorizontalSpan horizontalSpan = (HorizontalSpan)PlotMainSpanBeingDragged.Span;
                    Text text = FindAssociatedText(horizontalTextSpans, horizontalSpan);
                    if (text != null)
                    {
                        double y = sigMainCurr.Axes.YAxis.Range.Max;
                        double x1 = horizontalSpan.X2 > horizontalSpan.X1 ? horizontalSpan.X1 : horizontalSpan.X2;
                        double x2 = horizontalSpan.X2 > horizontalSpan.X1 ? horizontalSpan.X2 : horizontalSpan.X1;
                        text.Location = new Coordinates(x2, y);

                        double dt = x2 - x1;

                        int index1 = (int)((x1 * power_data_parama.sample_rate) % max_buffer_size);
                        int index2 = (int)((x2 * power_data_parama.sample_rate) % max_buffer_size);
                        double avg_i = (buffer.current_sum[index2] - buffer.current_sum[index1]) / (index2 - index1);
                        double min_i = double.MaxValue, max_i = double.MinValue;
                        double min_v = double.MaxValue, max_v = double.MinValue;

                        if(index1 < index2)
                        {
                            for (int i = index1; i < index2; i++)
                            {
                                if (buffer.current[i] < min_i)
                                {
                                    min_i = buffer.current[i];
                                }
                                else if (buffer.current[i] > max_i)
                                {
                                    max_i = buffer.current[i];
                                }
                                if (buffer.voltage[i] < min_v)
                                {
                                    min_v = buffer.voltage[i];
                                }
                                else if (buffer.voltage[i] > max_v)
                                {
                                    max_v = buffer.voltage[i];
                                }
                            }
                        }
                        else
                        {
                            for (int i = index1; i < max_buffer_size; i++)
                            {
                                if (buffer.current[i] < min_i)
                                {
                                    min_i = buffer.current[i];
                                }
                                else if (buffer.current[i] > max_i)
                                {
                                    max_i = buffer.current[i];
                                }
                                if (buffer.voltage[i] < min_v)
                                {
                                    min_v = buffer.voltage[i];
                                }
                                else if (buffer.voltage[i] > max_v)
                                {
                                    max_v = buffer.voltage[i];
                                }
                            }
                            for(int i =0;i< index2; i++)
                            {
                                if (buffer.current[i] < min_i)
                                {
                                    min_i = buffer.current[i];
                                }
                                else if (buffer.current[i] > max_i)
                                {
                                    max_i = buffer.current[i];
                                }
                                if (buffer.voltage[i] < min_v)
                                {
                                    min_v = buffer.voltage[i];
                                }
                                else if (buffer.voltage[i] > max_v)
                                {
                                    max_v = buffer.voltage[i];
                                }
                            }
                        }

                        double avg_v = (buffer.voltage_sum[index2] - buffer.voltage_sum[index1]) / (index2 - index1);
                        text.LabelText = "¦¤T  " + dt.ToString("0.0000") + " s\n";
                        string avg_str = "Avg: ";
                        string min_str = "Min: ";
                        string max_str = "Max: ";
                        if (checkBoxCurrent.Checked)
                        {
                            avg_str += FormatCurrent(avg_i / 1000) + " ";
                            min_str += FormatCurrent(min_i / 1000) + " ";
                            max_str += FormatCurrent(max_i / 1000) + " ";
                        }
                        if (checkBoxVolt.Checked)
                        {
                            avg_str += FormatVoltage(avg_v);
                            min_str += FormatVoltage(min_v);
                            max_str += FormatVoltage(max_v);
                        }
                        text.LabelText += avg_str +"\n"+ min_str + "\n" + max_str;
                    }
                }
                // currently dragging something so update it
                Coordinates mouseNow = formsPlotMain.Plot.GetCoordinates(e.X, e.Y);
                PlotMainSpanBeingDragged.DragTo(mouseNow);
                formsPlotMain.Refresh();
            }
            else
            {
                // not dragging anything so just set the cursor based on what's under the mouse
                var spanUnderMouse = FormsPlotMainGetSpanUnderMouse(e.X, e.Y);
                if (spanUnderMouse is null) Cursor = Cursors.Default;
                else if (spanUnderMouse.IsResizingHorizontally) Cursor = Cursors.SizeWE;
                else if (spanUnderMouse.IsResizingVertically) Cursor = Cursors.SizeNS;
                else if (spanUnderMouse.IsMoving) Cursor = Cursors.SizeAll;
            }

        }

        private AxisLine? FormsPlotMainGetLineUnderMouse(float x, float y)
        {
            CoordinateRect rect = formsPlotMain.Plot.GetCoordinateRect(x, y, radius: 10);

            foreach (AxisLine axLine in formsPlotMain.Plot.GetPlottables<AxisLine>().Reverse())
            {
                if (axLine.IsUnderMouse(rect))
                    return axLine;
            }

            return null;
        }

        private AxisSpanUnderMouse? FormsPlotMainGetSpanUnderMouse(float x, float y)
        {
            CoordinateRect rect = formsPlotMain.Plot.GetCoordinateRect(x, y, radius: 10);

            foreach (AxisSpan span in formsPlotMain.Plot.GetPlottables<AxisSpan>().Reverse())
            {
                AxisSpanUnderMouse? spanUnderMouse = span.UnderMouse(rect);
                if (spanUnderMouse is not null)
                    return spanUnderMouse;
            }

            return null;
        }
        static Text FindAssociatedText(List<HorizontalTextSpan> horizontalTextSpans, HorizontalSpan givenSpan)
        {
            foreach (var horizontaltextSpan in horizontalTextSpans)
            {
                if (givenSpan == horizontaltextSpan.span)
                {
                    return horizontaltextSpan.text;
                }
            }
            return null;
        }

        private AxisSpanUnderMouse? GetSpanUnderMouse(float x, float y)
        {
            CoordinateRect rect = formsPlotGuide.Plot.GetCoordinateRect(x, y, radius: 10);

            foreach (AxisSpan span in formsPlotGuide.Plot.GetPlottables<AxisSpan>().Reverse())
            {
                AxisSpanUnderMouse? spanUnderMouse = span.UnderMouse(rect);
                if (spanUnderMouse is not null)
                    return spanUnderMouse;
            }

            return null;
        }

        private void clear_data()
        {
            Array.Clear(mainDisData.data.current, 0, mainDisData.data.current.Length);
            Array.Clear(mainDisData.data.voltage, 0, mainDisData.data.voltage.Length);
            Array.Clear(guideDisData.data.current, 0, guideDisData.data.current.Length);
            Array.Clear(guideDisData.data.voltage, 0, guideDisData.data.voltage.Length);
            mainDisData.sample_factor = 1;
            guideDisData.sample_factor = 1;
            cur_recv_size = 0;
            clear_line_span();
            guideSpan.X1 = 0;
            guideSpan.X2 = guideDisData.data_size * guideDisData.sample_factor / power_data_parama.sample_rate;
            sigGuideCurr.Data.XOffset = 0;
            sigGuideVolt.Data.XOffset = 0;
        }
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                byte[] header_data = new byte[] { 0xab, 0xaa, 0xaa, 0xaa, 0x02, 0x00, 0x00, 0x00 };
                serialPort.Write(header_data, 0, 8);
                serialPort.Close();
                buttonOpen.Text = "Open";
                timerFlashPlot.Enabled = false;
                is_auto_follow = false;
                buttonFollow.Text = "Follow";
            }
            else
            {
                if (comboBoxPorts.SelectedItem != null)
                {
                    clear_data();
                    serialPort.PortName = comboBoxPorts.SelectedItem.ToString();
                    serialPort.BaudRate = 115200;
                    serialPort.Open();
                    byte[] header_data = new byte[] { 0xab, 0xaa, 0xaa, 0xaa, 0x01, 0x00, 0x00, 0x00 };
                    serialPort.Write(header_data, 0, 8);
                    buttonOpen.Text = "Close";
                    timerFlashPlot.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Please Select Port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void InitializeDeviceWatcher()
        {
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity'");
            ManagementEventWatcher watcher = new ManagementEventWatcher(scope, query);
            watcher.EventArrived += (sender, e) => UpdateSerialPorts();
            watcher.Start();

            WqlEventQuery removalQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity'");
            ManagementEventWatcher removalWatcher = new ManagementEventWatcher(scope, removalQuery);
            removalWatcher.EventArrived += (sender, e) => UpdateSerialPorts();
            removalWatcher.Start();
        }
        private void UpdateSerialPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            BeginInvoke(new Action(() =>
            {
                comboBoxPorts.Items.Clear();
                comboBoxPorts.Items.AddRange(ports);
            }));
        }
        private void InitializeSerialPorts()
        {
            serialPort = new SerialPort();
            serialPort.DataReceived += SerialPort_DataReceived;
            string[] ports = SerialPort.GetPortNames();
            comboBoxPorts.Items.AddRange(ports);
        }
        void serial_data_deal(byte[] data, int len)
        {
            byte[] header_data = new byte[] { 0xaa, 0xaa, 0xaa, 0xaa };
            bool isEqual = data.Take(4).SequenceEqual(header_data);
            uint buffer_addr = 0;
            if (isEqual == true)
            {
                uint payload_size = BitConverter.ToUInt16(data, 4);

                if (len == (payload_size * 2 + 2) * 4)
                {
                    for (int i = 0; i < payload_size; i++)
                    {
                        buffer_addr = (uint)(cur_recv_size + i) % max_buffer_size;
                        buffer.voltage[buffer_addr] = BitConverter.ToSingle(data, 8 + i * 8);
                        buffer.current[buffer_addr] = BitConverter.ToSingle(data, 12 + i * 8) * 1000.0f;
                        if (buffer_addr < max_buffer_size - 1)
                        {
                            buffer.current_sum[buffer_addr + 1] = buffer.current_sum[buffer_addr] + buffer.current[buffer_addr];
                            buffer.voltage_sum[buffer_addr + 1] = buffer.voltage_sum[buffer_addr] + buffer.voltage[buffer_addr];
                        }
                        else
                        {
                            buffer.current_sum[0] = buffer.current_sum[buffer_addr] + buffer.current[buffer_addr];
                            buffer.voltage_sum[0] = buffer.voltage_sum[buffer_addr] + buffer.voltage[buffer_addr];
                        }

                    }
                    cur_recv_size += payload_size;
                }

            }

        }
        int uart_received_len = 0;
        byte[] uart_buffer = new byte[10240];
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;

            // Read data to buffer
            sp.Read(uart_buffer, uart_received_len, bytesToRead);
            uart_received_len = uart_received_len + bytesToRead;
            if (uart_received_len > 511)
            {
                //Console.WriteLine("SerialPort_DataReceived uart_received_len" + uart_received_len);
                // Printf data receiver
                serial_data_deal(uart_buffer, uart_received_len);
                uart_received_len = 0;

            }
        }

        private void comboBoxPorts_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timerFlashPlot_Tick(object sender, EventArgs e)
        {

            double lim1 = guideSpan.X1 * guideDisData.sample_factor / power_data_parama.sample_rate;
            double lim2 = guideSpan.X2 * guideDisData.sample_factor / power_data_parama.sample_rate;
            Console.WriteLine("recv: " + cur_recv_size);
            if (cur_recv_size < guideDisData.data_size)
            {
                guideDisData.sample_factor = 1;
                for (int i = 0; i < cur_recv_size; i++)
                {
                    guideDisData.data.current[i] = buffer.current[i];
                    guideDisData.data.voltage[i] = buffer.voltage[i];
                }
            }
            else if (cur_recv_size < max_buffer_size)
            {
                guideDisData.sample_factor = (double)cur_recv_size / guideDisData.data_size;

                for (int i = 0; i < guideDisData.data_size; i++)
                {
                    guideDisData.data.current[i] = buffer.current[(int)(i * guideDisData.sample_factor)];
                    guideDisData.data.voltage[i] = buffer.voltage[(int)(i * guideDisData.sample_factor)];
                }
            }
            else
            {
                guideDisData.sample_factor = (double)max_buffer_size / guideDisData.data_size;
                //Console.WriteLine("guideDisData.sample_factor " + guideDisData.sample_factor);
                uint buffer_index_start = 0;
                uint buffer_index = 0;
                buffer_index_start = cur_recv_size % max_buffer_size;
                int i = 0;
                while (i < guideDisData.data_size)
                {
                    buffer_index = buffer_index_start + (uint)(i * guideDisData.sample_factor);
                    if (buffer_index >= max_buffer_size - 1)
                        break;
                    guideDisData.data.current[i] = buffer.current[buffer_index];
                    guideDisData.data.voltage[i] = buffer.voltage[buffer_index];
                    i++;
                }
                while (i < guideDisData.data_size)
                {
                    buffer_index = (uint)((i * guideDisData.sample_factor) + buffer_index_start) % max_buffer_size;
                    guideDisData.data.current[i] = buffer.current[buffer_index];
                    guideDisData.data.voltage[i] = buffer.voltage[buffer_index];
                    i++;
                }

            }

            if (!is_auto_follow)
            {
                double span_windows = guideSpan.X2 - guideSpan.X1;
                if (cur_recv_size > max_buffer_size)
                {
                    if (guideSpan.X1 < (cur_recv_size - max_buffer_size) / power_data_parama.sample_rate)
                    {
                        guideSpan.X1 = (cur_recv_size - max_buffer_size) / power_data_parama.sample_rate;
                    }
                    double X2 = guideSpan.X1 + span_windows;
                    guideSpan.X2 = (X2 > cur_recv_size / power_data_parama.sample_rate) ? cur_recv_size / power_data_parama.sample_rate : X2;
                    int startIndex = (int)(guideSpan.X1 * power_data_parama.sample_rate);
                    int endIndex = (int)(guideSpan.X2 * power_data_parama.sample_rate);
                    reselect_mainplot_datarange(startIndex, endIndex);
                }

            }
            else
            {

                if (guideSpan.X2 < cur_recv_size / power_data_parama.sample_rate)
                {
                    guideSpan.X2 = cur_recv_size / power_data_parama.sample_rate;
                }
                guideSpan.X1 = guideSpan.X2 - auto_follow_windows / power_data_parama.sample_rate;
                int startIndex = (int)(guideSpan.X1 * power_data_parama.sample_rate);
                int endIndex = (int)(guideSpan.X2 * power_data_parama.sample_rate);
                reselect_mainplot_datarange(startIndex, endIndex);
            }
            sigGuideCurr.Data.Period = guideDisData.sample_factor / power_data_parama.sample_rate;
            sigGuideVolt.Data.Period = guideDisData.sample_factor / power_data_parama.sample_rate;
            if (cur_recv_size > max_buffer_size)
            {
                sigGuideCurr.Data.XOffset = (cur_recv_size - max_buffer_size) / power_data_parama.sample_rate;
                sigGuideVolt.Data.XOffset = (cur_recv_size - max_buffer_size) / power_data_parama.sample_rate;
            }
            formsPlotGuide.Plot.Axes.AutoScaleY();
            formsPlotGuide.Plot.Axes.AutoScaleX();
            formsPlotGuide.Refresh();

        }

        private void buttonFollow_Click(object sender, EventArgs e)
        {
            if (is_auto_follow == false)
            {
                int startIndex = (int)(guideSpan.X1 * power_data_parama.sample_rate);
                int endIndex = (int)(guideSpan.X2 * power_data_parama.sample_rate);

                auto_follow_windows = endIndex - startIndex;
                if (auto_follow_windows < 0)
                {
                    auto_follow_windows = -auto_follow_windows;
                }
                is_auto_follow = true;
                buttonFollow.Text = "Stop Follow";
            }
            else
            {
                is_auto_follow = false;
                buttonFollow.Text = "Follow";
            }


        }

        private void buttonAddLine_Click(object sender, EventArgs e)
        {
            double center = sigMainCurr.Axes.XAxis.Range.Center;

            var vl = formsPlotMain.Plot.Add.VerticalLine(center);
            vl.IsDraggable = true;
            vl.LabelOppositeAxis = true;
            int index = (int)((vl.X - sigMainCurr.Data.XOffset) / sigMainCurr.Data.Period);
            double curr = sigMainCurr.Data.GetY(index);
            double volt = sigMainVolt.Data.GetY(index);
            vl.Text = $"T:{vl.X:0.00} C:{curr:0.00} V:{volt:0.00}";
            mainverticalLines.Add(vl);
            formsPlotMain.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double min = sigMainCurr.Axes.XAxis.Range.Min;
            double max = sigMainCurr.Axes.XAxis.Range.Max;
            double center = sigMainCurr.Axes.XAxis.Range.Center;

            var vl = formsPlotMain.Plot.Add.HorizontalSpan(center - (max - min) / 10, center + (max - min) / 10);
            vl.IsDraggable = true;
            vl.IsResizable = true;
            HorizontalTextSpan horizontalTextSpan = new HorizontalTextSpan();
            horizontalTextSpan.span = vl;
            double y = sigMainCurr.Axes.YAxis.Range.Max;
            Text text = formsPlotMain.Plot.Add.Text("", center + (max - min) / 10, y);
            text.Label.FontSize = 18;
            text.OffsetY = 5;
            text.Label.ForeColor = Colors.Yellow;
            text.Label.Alignment = Alignment.UpperLeft;

            horizontalTextSpan.text = text;
            horizontalTextSpans.Add(horizontalTextSpan);
            formsPlotMain.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void formsPlotMain_Load(object sender, EventArgs e)
        {

        }
        private void clear_line_span()
        {
            foreach (VerticalLine line in mainverticalLines)
            {
                formsPlotMain.Plot.Remove(line);
            }
            foreach (HorizontalTextSpan span in horizontalTextSpans)
            {
                formsPlotMain.Plot.Remove(span.span);
                formsPlotMain.Plot.Remove(span.text);
            }
            mainverticalLines.Clear();
            horizontalTextSpans.Clear();

            formsPlotMain.Refresh();
        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            clear_line_span();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            sigGuideCurr.IsVisible = checkBoxCurrent.Checked;
            sigMainCurr.IsVisible = checkBoxCurrent.Checked;
            formsPlotMain.Refresh();
            formsPlotGuide.Refresh();
        }

        private void checkBoxVolt_CheckedChanged(object sender, EventArgs e)
        {
            sigGuideVolt.IsVisible = checkBoxVolt.Checked;
            sigMainVolt.IsVisible = checkBoxVolt.Checked;
            formsPlotMain.Refresh();
            formsPlotGuide.Refresh();
        }
    }
}