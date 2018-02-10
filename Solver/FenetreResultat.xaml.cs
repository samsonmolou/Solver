using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Solver
{
    /// <summary>
    /// Logique d'interaction pour FenetreResultat.xaml
    /// </summary>
    public partial class FenetreResultat : Window
    {
        public FenetreResultat()
        {
            InitializeComponent();
        }

        private void buttonQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }   
    }
}
