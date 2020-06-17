using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DlibDotNet;
using Newtonsoft.Json;
using Rectangle = DlibDotNet.Rectangle;
using Syncfusion.XlsIO;

namespace ifadeTespitEtme
{
    public partial class Form1 : Form
    {
        int inputCount = 1;

        OpenFileDialog open = new OpenFileDialog();
        string[] secilenFotolar;

        //pictureBox başlangıç noktaları
        int x = 30;
        int y = 30;
        int qx = 30;
        int qy = 30;
        int maxHeight = -1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fotoAyarlari();
        }

        private void fotoAyarlari()
        {
            this.open.Title = "Fotoğraf(lar) seçiniz...";
            this.open.Multiselect = true;
            this.open.Filter =
                "Fotoğraflar |*.jpeg;*.jpg;*.png|" +
                "Tüm dosyalar |*.*";            
        }


        private void btnFotoYukle_Click(object sender, EventArgs e)
        {
            
            if (open.ShowDialog() == DialogResult.OK)
            {
                groupBox1.Enabled = true;

                //pictureBox1.Image = new Bitmap(open.FileName);
                secilenFotolar = open.FileNames;

                foreach (var foto in secilenFotolar)
                {
                    PictureBox pic = new PictureBox();
                    pic.Image = Image.FromFile(foto);
                    pic.Location = new System.Drawing.Point(x, y);
                    pic.SizeMode = PictureBoxSizeMode.StretchImage;
                    pic.Height = 98;
                    pic.Width = 128;
                    x += pic.Width + 10;
                    maxHeight = Math.Max(pic.Height, maxHeight);
                    if (x>this.ClientSize.Width-300)
                    {
                        x = 30;
                        y += maxHeight + 10;
                    }
                    this.panel4.Controls.Add(pic);

                    lblFotoAdet.Text = secilenFotolar.Length.ToString();
                    lblOrjinalYol.Text = foto;
                }
            }

        }

        DataTable dt = new DataTable("Inputs");
        DataTable dt2 = new DataTable("Outputs");

        private void btnCalistir_Click(object sender, EventArgs e)//calistir butonu
        {
            panel4.Controls.Clear();
            btnFotoYukle.Enabled = false;
            groupBox1.Enabled = false;
            btnCalistir.Enabled = false;

            using (var fd = Dlib.GetFrontalFaceDetector())
            using (var sp = ShapePredictor.Deserialize("shape_predictor_68_face_landmarks.dat"))
            {
                foreach (var foto in secilenFotolar)
                {
                    lblOrjinalYol.Text = foto;
                    
                    // Load image from file
                    var img = Dlib.LoadImage<RgbPixel>(foto);

                    // Detect all faces
                    var faces = fd.Operator(img);
                    int yuzAdeti = 0;
                    if (faces.Length > 0)
                    {
                        foreach (var face in faces)
                        {
                            yuzAdeti++;
                            // Find the landmark points for this face
                            var shape = sp.Detect(img, face);
                            //Console.WriteLine("ilk yuz icin landmarklar"+ shape.ToString());
                            // Loop through detected landmarks
                            string path = @"d:\landmarks.txt";
                            data dataObjem = new data();
                            dataObjem.Values = new double[136];
                            dataObjem.Targets = new double[7];


                            for (int i = 0; i < shape.Parts; i++)
                            {
                                var point = shape.GetPart((uint)i);

                                dataObjem.Values[i] = point.X;

                                var rect = new Rectangle(point);
                                Dlib.DrawRectangle(img, rect, color: new RgbPixel(255, 0, 0), thickness: 8);

                                // Console.WriteLine(shape.NativePtr.ToString());
                            }

                            for (int i = 0; i < shape.Parts; i++)
                            {
                                //console.writeline(shape.tostring());
                                var point = shape.GetPart((uint)i);

                                dataObjem.Values[i + 68] = point.Y;
                                //write string to file
                            }


                            if (rdbtnAnger.Checked)
                                dataObjem.Targets[0] = 1.0;

                            else if (rdbtnDisgust.Checked)
                                dataObjem.Targets[1] = 1.0;

                            else if (rdbtnFear.Checked)
                                dataObjem.Targets[2] = 1.0;

                            else if (rdbtnHappy.Checked)
                                dataObjem.Targets[3] = 1.0;

                            else if (rdbtnSad.Checked)
                                dataObjem.Targets[4] = 1.0;

                            else if (rdbtnSurprise.Checked)
                                dataObjem.Targets[5] = 1.0;

                            else if (rdbtnComtempt.Checked)
                                dataObjem.Targets[6] = 1.0;

                            string json = JsonConvert.SerializeObject(dataObjem, Formatting.Indented);

                            using (ExcelEngine excelEngine = new ExcelEngine())
                            {
                                IApplication application = excelEngine.Excel;
                                application.DefaultVersion = ExcelVersion.Excel2013;
                                IWorkbook workbook = excelEngine.Excel.Workbooks.Open("C:\\Users\\Enes\\Desktop\\test.xlsx");
                                IWorksheet InputWorksheet = workbook.Worksheets[0];
                                IWorksheet OutputWorksheet = workbook.Worksheets[1];

                                //Import the Object Array to Sheet
                                InputWorksheet.ImportArray(dataObjem.Values, 1, inputCount, true);
                                OutputWorksheet.ImportArray(dataObjem.Targets, 1, inputCount, true);
                                inputCount++;

                                //Saving the workbook to disk in XLSX format
                                workbook.Save();
                                //Close the instance of IWorkbook
                                workbook.Close();

                                //Dispose the instance of ExcelEngine
                                excelEngine.Dispose();

                            }

                            using (StreamWriter w = File.AppendText(path))
                            {
                                w.WriteLine(json);
                                w.WriteLine(',');
                            }

                            string fotoYolu = foto.ToString();

                            char[] splitchar = { '.' };
                            string[] parcalanmisYol = fotoYolu.Split(splitchar);
                            string fotoilkAd = parcalanmisYol[0];

                            string yeniFotoYol = fotoilkAd + "-Result.jpg";

                            lblDegistirilmisYol.Text = yeniFotoYol;
                            Dlib.SaveJpeg(img, yeniFotoYol);



                            PictureBox pic = new PictureBox();
                            pic.Image = Image.FromFile(yeniFotoYol);
                            pic.Location = new System.Drawing.Point(qx, qy);
                            pic.SizeMode = PictureBoxSizeMode.StretchImage;
                            pic.Height = 98;
                            pic.Width = 128;
                            qx += pic.Width + 10;
                            maxHeight = Math.Max(pic.Height, maxHeight);
                            if (qx > this.ClientSize.Width - 300)
                            {
                                qx = 30;
                                qy += maxHeight + 10;
                            }
                            this.panel4.Controls.Add(pic);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Yüz bulunamadı...");
                    }
                }//end foreach
            }//end using
        }//end btnCalistir

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            btnCalistir.Enabled = true;
        }

        private void btnCalistir_MouseDown(object sender, MouseEventArgs e)
        {
            lblBilgi1.Visible = true;
        }

        private void lblDegistirilmisYol_TextChanged(object sender, EventArgs e)
        {
            lblBilgi1.Visible = false;
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            lblFotoAdet.Text = "---";
            lblOrjinalYol.Text = "-";
            lblDegistirilmisYol.Text = "-";
            btnFotoYukle.Enabled = true;
            x = 30;
            y = 30;

            qx = 30;
            qy = 30;

            maxHeight = -1;

        }
    }//end form
}//end namespace

