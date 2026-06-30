using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageAnalyzer
{
    public partial class Form1 : Form
    {
        [DllImport("Model.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool CargarModelo(string modelPath);

        [DllImport("Model.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool ClasificarImagen(string imagePath, out int classId, out float confidence);

        [DllImport("Model.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool ClasificarFotograma(IntPtr pixelBuffer, int width, int height, int channels, out int classId, out float confidence);
        string modelPath = "";
        string imagePath = "";
        int idResultado;
        float confianzaResultado;
        bool cargado = false;
        int currentCam = 0;
        private FilterInfoCollection misDispositivos;
        private VideoCaptureDevice miWebcam;
        public Form1()
        {
            misDispositivos = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            InitializeComponent();
            devices.Items.Add("Seleccionar cámara...");
            for (int i = 0; i < misDispositivos.Count; i++)
            {
                devices.Items.Add(misDispositivos[i].Name);
            }
            devices.SelectedIndex = 0;            
        }

        private void of_Click(object sender, EventArgs e)
        {
            if (!cargado)
            {
                MessageBox.Show("Es necesario cargar el modelo", "Error");
            }
            else
            {
                if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    imagePath = openFileDialog1.FileName;
                    if (ClasificarImagen(imagePath, out idResultado, out confianzaResultado))
                    {
                        if(miWebcam != null)
                        {
                            miWebcam.SignalToStop();
                        }
                        if (idResultado == 0)
                            clase.Text = "FRUTA VERDE";
                        else
                            clase.Text = "FRUTA MADURA";
                        confianzatxt.Text = "CONFIANZA: " + confianzaResultado * 100 + "%";
                        image.Image = Image.FromFile(imagePath);
                        imgp.Text = imagePath;
                    }
                    else
                    {
                        clase.Text = "";
                        confianzatxt.Text = "";
                        MessageBox.Show("No se pudo clasificar la imagen", "Error");
                        image.Image = null;
                        imgp.Text = "";
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog(this) == DialogResult.OK)
            {
                modelPath = openFileDialog2.FileName;
                clase.Text = "";
                confianzatxt.Text = "";
                if (CargarModelo(modelPath))
                {
                    modelp.Text = modelPath;
                    cargado = true;
                }
                else
                {
                    MessageBox.Show("No se pudo cargar el modelo", "Error");
                    cargado = false;
                }
            }
        }
        private void NuevoFotograma(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    BitmapData bmpData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb
                    );
                    bool exito = ClasificarFotograma(bmpData.Scan0, bitmap.Width, bitmap.Height, 3, out idResultado, out confianzaResultado);
                    bitmap.UnlockBits(bmpData);
                    if (exito)
                    {
                        if (!this.Disposing && !this.IsDisposed)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                if (idResultado == 0)
                                    clase.Text = "FRUTA VERDE";
                                else
                                    clase.Text = "FRUTA MADURA";
                                confianzatxt.Text = "CONFIANZA: " + confianzaResultado * 100 + "%";
                                image.Image = (Bitmap)bitmap.Clone();
                            }));
                        }
                    }
                }
            } catch { }
                       
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!cargado)
            {
                MessageBox.Show("Es necesario cargar el modelo", "Error");
            }
            else
            {
                if (devices.SelectedIndex > 0)
                {
                    if(miWebcam == null || devices.SelectedIndex != currentCam)
                    {
                        miWebcam = new VideoCaptureDevice(misDispositivos[devices.SelectedIndex - 1].MonikerString);
                        miWebcam.NewFrame += new NewFrameEventHandler(NuevoFotograma);
                        miWebcam.Start();
                        imgp.Text = "";
                        imagePath = "";
                        currentCam = devices.SelectedIndex;
                    }
                    else
                    {
                        miWebcam.Start();
                    }

                }
                else
                {
                    MessageBox.Show("Es necesario seleccionar una cámara", "Error");
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (miWebcam != null)
            {
                miWebcam.NewFrame -= NuevoFotograma;
                miWebcam.SignalToStop();
            }
        }
    }
}
