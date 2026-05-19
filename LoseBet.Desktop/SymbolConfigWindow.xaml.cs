using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace LoseBet.Desktop
{
    public partial class SymbolConfigWindow : Window
    {
        public SymbolConfigDTO ConfiguredSymbol { get; private set; }
        private string _fullFilePath;

        public SymbolConfigWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Imagini (*.png;*.jpg)|*.png;*.jpg" };
            if (op.ShowDialog() == true)
            {
                _fullFilePath = op.FileName;
                TxtImageFile.Text = Path.GetFileName(_fullFilePath);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_fullFilePath))
            {
                MessageBox.Show("Te rog să selectezi o imagine!");
                return;
            }

            try
            {
                // Copiem fișierul fizic în resursele jocului
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Slots");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string newFileName = Path.GetFileName(_fullFilePath);
                File.Copy(_fullFilePath, Path.Combine(folder, newFileName), true);

                // Creăm obiectul cu datele financiare ale simbolului
                ConfiguredSymbol = new SymbolConfigDTO
                {
                    ImageName = newFileName,
                    Multiplier3 = decimal.Parse(TxtMult3.Text),
                    Multiplier4 = decimal.Parse(TxtMult4.Text),
                    Multiplier5 = decimal.Parse(TxtMult5.Text),
                    IsWild = ChkIsWild.IsChecked ?? false
                };

                this.DialogResult = true; // Semnalăm că s-a salvat cu succes
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: Verificați dacă multiplicatorii sunt scriși corect (numere) \n" + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    // Punem DTO-ul aici dacă nu îl citește direct din API
    public class SymbolConfigDTO
    {
        public string ImageName { get; set; } = string.Empty;
        public decimal Multiplier3 { get; set; }
        public decimal Multiplier4 { get; set; }
        public decimal Multiplier5 { get; set; }
        public bool IsWild { get; set; }
    }
}