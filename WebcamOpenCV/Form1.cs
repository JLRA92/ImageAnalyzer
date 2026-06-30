using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

using DrawingPoint = System.Drawing.Point;
using DrawingSize = System.Drawing.Size;

namespace WebcamOpenCV
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection dispositivos;
        private VideoCaptureDevice camara;
        private Bitmap imagenCapturada;

        private PictureBox pictureBoxOriginal;

        private Button btnCapturar;
        private Button btnGuardar;
        private Button btnSalir;

        private GroupBox groupEstado;
        private RadioButton rbMadura;
        private RadioButton rbVerde;

        private Label lblOriginal;
        private Label lblMensaje;

        public Form1()
        {
            InitializeComponent();
            CrearInterfaz();
            this.Load += Form1_Load;
        }

        private void CrearInterfaz()
        {
            Text = "Captura de muestras";
            ClientSize = new DrawingSize(520, 630);
            StartPosition = FormStartPosition.CenterScreen;

            lblOriginal = new Label();
            lblOriginal.Text = "Imagen de la muestra";
            lblOriginal.Location = new DrawingPoint(30, 20);
            lblOriginal.Size = new DrawingSize(250, 25);

            pictureBoxOriginal = new PictureBox();
            pictureBoxOriginal.Location = new DrawingPoint(30, 50);
            pictureBoxOriginal.Size = new DrawingSize(450, 400);
            pictureBoxOriginal.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.Zoom;

            groupEstado = new GroupBox();
            groupEstado.Text = "Clasificación";
            groupEstado.Location = new DrawingPoint(30, 465);
            groupEstado.Size = new DrawingSize(250, 70);

            rbMadura = new RadioButton();
            rbMadura.Text = "Madura";
            rbMadura.Location = new DrawingPoint(20, 30);
            rbMadura.Size = new DrawingSize(90, 25);

            rbVerde = new RadioButton();
            rbVerde.Text = "Verde";
            rbVerde.Location = new DrawingPoint(130, 30);
            rbVerde.Size = new DrawingSize(90, 25);

            groupEstado.Controls.Add(rbMadura);
            groupEstado.Controls.Add(rbVerde);

            btnCapturar = new Button();
            btnCapturar.Text = "Tomar fotografía";
            btnCapturar.Location = new DrawingPoint(30, 550);
            btnCapturar.Size = new DrawingSize(140, 35);
            btnCapturar.Click += BtnCapturar_Click;

            btnGuardar = new Button();
            btnGuardar.Text = "Guardar imagen";
            btnGuardar.Location = new DrawingPoint(190, 550);
            btnGuardar.Size = new DrawingSize(140, 35);
            btnGuardar.Enabled = false;
            btnGuardar.Click += BtnGuardar_Click;

            btnSalir = new Button();
            btnSalir.Text = "Salir";
            btnSalir.Location = new DrawingPoint(350, 550);
            btnSalir.Size = new DrawingSize(100, 35);
            btnSalir.Click += BtnSalir_Click;

            lblMensaje = new Label();
            lblMensaje.Text = "Cámara lista";
            lblMensaje.Location = new DrawingPoint(30, 600);
            lblMensaje.Size = new DrawingSize(450, 25);

            Controls.Add(lblOriginal);
            Controls.Add(pictureBoxOriginal);
            Controls.Add(groupEstado);
            Controls.Add(btnCapturar);
            Controls.Add(btnGuardar);
            Controls.Add(btnSalir);
            Controls.Add(lblMensaje);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IniciarCamara();
        }

        private void IniciarCamara()
        {
            dispositivos = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            FilterInfo camaraSeleccionada = dispositivos[0];

            foreach (FilterInfo dispositivo in dispositivos)
            {
                if (dispositivo.Name.Contains("Logi") || dispositivo.Name.Contains("C270"))
                {
                    camaraSeleccionada = dispositivo;
                    break;
                }
            }

            camara = new VideoCaptureDevice(camaraSeleccionada.MonikerString);
            camara.NewFrame += Camara_NewFrame;
            camara.Start();

            lblMensaje.Text = "Cámara lista";
        }

        private void Camara_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

            if (pictureBoxOriginal.InvokeRequired)
            {
                pictureBoxOriginal.BeginInvoke(new Action(() =>
                {
                    MostrarFrame(frame);
                }));
            }
            else
            {
                MostrarFrame(frame);
            }
        }

        private void MostrarFrame(Bitmap frame)
        {
            if (imagenCapturada == null)
            {
                if (pictureBoxOriginal.Image != null)
                {
                    pictureBoxOriginal.Image.Dispose();
                }

                pictureBoxOriginal.Image = frame;
            }
            else
            {
                frame.Dispose();
            }
        }

        private void BtnCapturar_Click(object sender, EventArgs e)
        {
            if (imagenCapturada == null)
            {
                imagenCapturada = new Bitmap(pictureBoxOriginal.Image);

                btnCapturar.Text = "Volver a cámara";
                btnGuardar.Enabled = true;
                lblMensaje.Text = "Imagen tomada";
            }
            else
            {
                imagenCapturada.Dispose();
                imagenCapturada = null;

                btnCapturar.Text = "Tomar fotografía";
                btnGuardar.Enabled = false;
                lblMensaje.Text = "Cámara lista";
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (imagenCapturada == null)
            {
                MessageBox.Show("Primero toma una fotografía.");
                return;
            }

            if (!rbMadura.Checked && !rbVerde.Checked)
            {
                MessageBox.Show("Selecciona una clasificación.");
                return;
            }

            string carpetaBase = AppDomain.CurrentDomain.BaseDirectory;
            string carpetaDestino;
            string prefijo;

            if (rbMadura.Checked)
            {
                carpetaDestino = Path.Combine(carpetaBase, "M");
                prefijo = "m";
            }
            else
            {
                carpetaDestino = Path.Combine(carpetaBase, "V");
                prefijo = "v";
            }

            Directory.CreateDirectory(carpetaDestino);

            string fecha = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string nombreArchivo = prefijo + fecha + ".jpeg";
            string ruta = Path.Combine(carpetaDestino, nombreArchivo);

            imagenCapturada.Save(ruta, System.Drawing.Imaging.ImageFormat.Jpeg);

            MessageBox.Show("Imagen guardada:\n" + ruta);

            imagenCapturada.Dispose();
            imagenCapturada = null;

            btnCapturar.Text = "Tomar fotografía";
            btnGuardar.Enabled = false;
            lblMensaje.Text = "Imagen guardada";
        }

        private void BtnSalir_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DetenerCamara()
        {
            if (camara != null && camara.IsRunning)
            {
                camara.SignalToStop();
                camara.WaitForStop();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            DetenerCamara();

            if (imagenCapturada != null)
            {
                imagenCapturada.Dispose();
            }

            if (pictureBoxOriginal.Image != null)
            {
                pictureBoxOriginal.Image.Dispose();
            }

            base.OnFormClosing(e);
        }
    }
}