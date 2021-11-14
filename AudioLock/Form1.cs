using System.Globalization;
using CSCore.CoreAudioAPI;
using CSCore.Win32;

namespace AudioUnfuck
{

    public partial class Form1 : Form
    {
        private static Guid DEVPKEY_DeviceClass = new Guid("259abffc-50a7-47ce-af08-68c9a7d73366");
        private static int IconPathIndex = 12;

        private MMDevice[] captureDevices;
        private ImageList imageList;
        private bool[] locked;
        private LockManager lockManager;

        public Form1()
        {
#if DEBUG
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
#endif
            InitializeComponent();
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var endpoints = enumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
            imageList = new ImageList();
            captureDevices = endpoints.ToArray();
            locked = new bool[endpoints.Count];
            lockManager = new LockManager();
        }

        private Icon GetIconFromPath(String path)
        {
            var parts = path.Split(',');
            if (parts.Length == 1)
            {
                return new Icon(parts[0]);
            }
            else if (parts.Length == 2)
            {
                return IconExtractor.ExtractIcon(parts[0], Int32.Parse(parts[1]), true);
            }
            return SystemIcons.WinLogo;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ColumnHeader chIcon, chName, chLevel;
            chIcon = new ColumnHeader();
            chName = new ColumnHeader();
            chLevel = new ColumnHeader();
            chIcon.Text = "";
            chName.Text = "Name";
            chLevel.Text = "Level";
            chLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            chLevel.Width = 40;

            listView1.Columns.AddRange(new ColumnHeader[] { chIcon, chName, chLevel });

            imageList.ImageSize = new Size(32, 32);
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            listView1.SmallImageList = imageList;


            ListViewItem item;
            listView1.BeginUpdate();

            foreach (MMDevice device in captureDevices)
            {
                AudioEndpointVolume endpointVolume = AudioEndpointVolume.FromDevice(device);

                PropertyStore propertyStore = device.OpenPropertyStore(StorageAccess.Read);
                PropertyVariant iconPathProp = propertyStore.GetValue(new PropertyKey(DEVPKEY_DeviceClass, IconPathIndex));
                String iconPath = iconPathProp.ToString();

                Icon icon = SystemIcons.WinLogo;

                item = new ListViewItem();

                if (!imageList.Images.ContainsKey(iconPath))
                {
                    icon = GetIconFromPath(iconPath);
                    imageList.Images.Add(iconPath, icon);
                }
                item.ImageKey = iconPath;

                item.SubItems.Add(device.FriendlyName);
                item.SubItems.Add(endpointVolume.MasterVolumeLevelScalar.ToString("0.00"));
                listView1.Items.Add(item);
                chIcon.Width = -1;
                chName.Width = -1;
            }
            listView1.EndUpdate();
        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == CheckState.Unchecked)
            {
                lockManager.Subscribe(captureDevices[e.Index]);
            }
            else if ((e.CurrentValue == CheckState.Checked))
            {
                lockManager.Unsubscribe(captureDevices[e.Index]);
            }
        }
    }
}
