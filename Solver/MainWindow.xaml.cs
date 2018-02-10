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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Globalization;

namespace Solver
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonResoudre_Click(object sender, RoutedEventArgs e)
        {
            string s_equationSystem = null;
            string s_initialEquation = null;
            s_equationSystem = textBoxEquationSystem.Text;
            s_initialEquation = textBoxEquationInitiale.Text;
            Message("Starting...");
            Message(s_equationSystem);
            String[] t_equationSystem = s_equationSystem.Split(new String[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            Message("Vérification du système d'équation...");
            if (!SystemVerify(t_equationSystem))
                Message("Le système n'est pas valide ! :-(");
            else
            {
                Message("Le système d'équation est valide");
                Message("Vérification de la matrice initiale !");
                if (!InitialVerify(s_initialEquation, ListeVariable(t_equationSystem)))
                {
                    Message("L'équation initiale n'est pas valide !");
                    return;
                }
                Message("Récupération de la liste des variables");
                List<char> l_variable = ListeVariable(t_equationSystem);
                string s_temp = null;
                foreach (char s in l_variable)
                {
                    s_temp += s;
                }

                Message("Liste des variables : " + s_temp);
                int i_nbLigne = NombreLigne(t_equationSystem);
                int i_nbColonne = NombreColonne(t_equationSystem);

                Message("Ligne : " + i_nbLigne.ToString() + " Colonne : " + i_nbColonne.ToString());

                if (i_nbLigne != i_nbColonne)
                {
                    Message("Le nombre de ligne doit être égal au nombre de colonne");
                    return;
                }
                    
                Dictionary<Tuple<int, string>, float> d_matriceA = new Dictionary<Tuple<int, string>, float>();
                Dictionary<int, float> d_matriceB = new Dictionary<int, float>();
                Dictionary<string, float> d_matriceX = new Dictionary<string, float>();
                Dictionary<string, float> d_matriceY = new Dictionary<string, float>();

                d_matriceA = InitMatriceA(t_equationSystem);
                d_matriceB = InitMatriceB(t_equationSystem);
                d_matriceX = InitMatriceX(s_initialEquation);

                int i_nbIteration = 0;
                try {
                    i_nbIteration = Convert.ToInt32(textBoxNbIteration.Text);
                }
                catch(Exception ex)
                {

                    Message("Erreur : " + ex.Message);
                }
                double i_tolerance = 0;
                try
                {
                    i_tolerance = Convert.ToDouble(textBoxTolerance.Text);
                }
                catch(Exception ex)
                {
                    Message("Erreur : " + ex.Message);
                }
                GaussSeidel gaussSeidel = new GaussSeidel(d_matriceA, d_matriceB, d_matriceX, i_nbIteration, l_variable,i_tolerance);

                string s_solution = gaussSeidel.Resoudre();

                
                Message(s_solution);
                labelTempsExecution.Content = gaussSeidel.tempsExecution();
            }
        }

        private Dictionary<string, float> InitMatriceX(string pInitialEquation)
        {
            Dictionary<string, float> d_matriceX = new Dictionary<string, float>();
            string temp = SupprimerEspace(pInitialEquation);
            string[] s_temp = temp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string s in s_temp)
            {
                string[] s_temp1 = s.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (s_temp1.Length == 2)
                    d_matriceX[s_temp1[0]] = (float)Convert.ToDouble(s_temp1[1]);
                else
                    throw new Exception("Erreur de l'extration de la matrice initial");
            }
            return d_matriceX;
        }

        private bool InitialVerify(string pEquationInitial, List<char> pListeVariable)
        {
            string s_temp = SupprimerEspace(pEquationInitial);
            List<char> l_listeCaractere = ExtractCaractere(s_temp);
            foreach (char c in l_listeCaractere)
            {
                if (!pListeVariable.Contains(c))
                {
                    return false;
                }
            }
            int i_suivant = 1;
            int i_courant = 0;
            string s_elementSuivant = null;
            string s_elementCourant = null;
            
            while (i_suivant < s_temp.Length)
            {
                s_elementCourant = s_temp[i_courant].ToString();
                s_elementSuivant = s_temp[i_suivant].ToString();
                switch (TypeElement(s_elementCourant))
                {
                    //l'élément est un caractère x, y, z par exemple
                    case 1:
                        if (!EstEgale(s_elementSuivant))
                            return false;
                        break;
                    //l'élement est un chiffre
                    case 2:
                        if (!EstVirgule(s_elementSuivant))
                            return false;
                        break;
                    //l'élément est une virgule
                    case 6:
                        if (!EstCaractere(s_elementSuivant))
                            return false;
                        break;
                    //l'élément est le signe =
                    case 5:
                        if (!EstChiffre(s_elementSuivant))
                            return false;
                        break;
                    default:
                        throw new Exception("Le type ne peut pas être déterminée");
                }
                i_courant++;
                i_suivant++;
            }
            return true;
        }

        private Dictionary<int, float> InitMatriceB(string[] pEquationSystem)
        {
            Dictionary<int, float> d_matriceB = new Dictionary<int, float>();
            int i_ligne = 0;
            foreach (string equation in pEquationSystem)
            {
                string[] s_temp = equation.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (s_temp.Length == 2)
                    d_matriceB[i_ligne] = (float)Convert.ToDouble(SupprimerEspace(s_temp[1]), new CultureInfo("en-US"));
                else
                    throw new Exception("Erreur lors de l'extraction des coefficients de la matrice B");
                i_ligne++;
            }
            return d_matriceB;
        }

        private Dictionary<Tuple<int, string>, float> InitMatriceA(string[] pEquationSystem)
        {
            List<char> l_variables = ListeVariable(pEquationSystem);
            Dictionary<Tuple<int, string>, float> d_matriceA = new Dictionary<Tuple<int, string>, float>();
            string s_temp = null;
            int i_ligne = 0;
            foreach (string equation in pEquationSystem)
            {
                foreach (char s in l_variables)
                {
                    s_temp = SupprimerEspace(equation);
                    s_temp = CoefficientVariable(s_temp, s);
                    
                    d_matriceA[Tuple.Create(i_ligne, s.ToString())] = (float)Convert.ToDouble(s_temp, new CultureInfo("en-US"));
                    s_temp = null;
                    
                }
                i_ligne++;
            }
            return d_matriceA;
        }

        private string CoefficientVariable(string pEquation, char pVariable)
        {
            string s_coefficient = null;

            if (!pEquation.Contains(pVariable))
            {
                s_coefficient = "0";
                return s_coefficient;
            }

            foreach (char c in pEquation)
            {
                if (EstOperateur(c.ToString()))
                {
                    if (c == '-')
                        s_coefficient = "-";
                    else
                        s_coefficient = null;
                }
                else if (!c.Equals(pVariable))
                    s_coefficient += c;
                else
                    break;
            }
            if (s_coefficient == null)
                s_coefficient = "1";
            else if (s_coefficient == "-")
                s_coefficient = "-1";
            return s_coefficient;
        }

        private int NombreLigne(string[] pEquationSystem)
        {
            return pEquationSystem.Length;
        }

        private int NombreColonne(string[] pEquationSystem)
        {
            List<char> l_variables = new List<char>();
            l_variables = ListeVariable(pEquationSystem);
            return l_variables.Count;
        }

        private List<char> ListeVariable(String[] pEquationSystem)
        {
            List<char> l_listeVariable = new List<char>();

            foreach (string s in pEquationSystem)
            {
                foreach (char c in s)
                {
                    if (EstCaractere(c.ToString()))
                        if (!l_listeVariable.Contains(c))
                            l_listeVariable.Add(c);
                }
            }
            return l_listeVariable;
        }

        private List<char> ExtractCaractere(string s)
        {
            List<char> l_caractere = new List<char>();
            foreach(char c in s)
            {
                if (EstCaractere(c.ToString()))
                    if (!l_caractere.Contains(c))
                        l_caractere.Add(c);
            }
            return l_caractere;
        }

        private bool SystemVerify(String[] pEquationSystem)
        {
            string s_elementCourant = null;
            string s_elementSuivant = null;
            int i_suivant = 1;
            int i_courant = 0;

            foreach (string s in pEquationSystem)
            {
                string s_temp = SupprimerEspace(s);
                i_courant = 0;
                i_suivant = 1;
                s_elementCourant = s_temp[i_courant].ToString();
                s_elementSuivant = s_temp[i_suivant].ToString();
                while (i_suivant < s_temp.Length)
                {
                    s_elementCourant = s_temp[i_courant].ToString();
                    s_elementSuivant = s_temp[i_suivant].ToString();
                    switch (TypeElement(s_elementCourant))
                    {
                        
                        //l'element est un caractère x, y, z par exemple
                        case 1:
                            if (!EstOperateur(s_elementSuivant) && !EstEgale(s_elementSuivant))
                                return false;
                            break;
                        //l'élement est un chiffre
                        case 2:
                            if (EstOperateur(s_elementSuivant))
                                return false;
                            break;
                        //l'élément est un opérateur
                        case 3:
                            if (EstOperateur(s_elementSuivant))
                                return false;
                            break;
                        //l'élément est une virgule
                        case 4:
                            if (!EstChiffre(s_elementSuivant))
                                return false;
                            break;
                        //l'élément est le signe =
                        case 5:
                            if (EstCaractere(s_elementSuivant) && EstVirguleMathematique(s_elementSuivant))
                                return false;
                            break;
                        default:
                            throw new Exception("Le type ne peut pas être déterminée");
                    }
                    i_courant++;
                    i_suivant++;
                }
            }
            return true;
        }

        private string SupprimerEspace(string pChaine)
        {
            string res = null;
            foreach (char s in pChaine)
            {
                if (s != ' ')
                    res += s;
            }
            return res;
        }
        private bool EstChiffre(string s)
        {
            String[] t_chiffre = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            return t_chiffre.Contains(s);
        }

        private bool EstOperateur(string s)
        {
            String[] t_operateur = { "+", "-" };

            return t_operateur.Contains(s);
        }

        private bool EstCaractere(string s)
        {
            String[] t_caractere = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

            return t_caractere.Contains(s);
        }

        private bool EstVirguleMathematique(string s)
        {
            return s.Equals(".");
        }

        private bool EstVirgule(string s)
        {
            return s.Equals(",");
        }

        private bool EstEgale(string s)
        {
            return s.Equals("=");
        }

        private int TypeElement(string s)
        {
            if (EstCaractere(s))
                return 1;
            else if (EstChiffre(s))
                return 2;
            else if (EstOperateur(s))
                return 3;
            else if (EstVirguleMathematique(s))
                return 4;
            else if (EstEgale(s))
                return 5;
            else if (EstVirgule(s))
                return 6;
            else
                return -1;
        }

        private void Message(string pMessage)
        {
            labelResultat.Content = pMessage;
        }

		/// <summary>
		/// Cette fonction calcule la matrice initiale. Elle est déclenchée lorsqu'on sort de la zone de saisie du système.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textBoxEquationSystem_LostFocus(object sender, RoutedEventArgs e)
		{
			string s_matriceX = null;

			String[] t_equationSystem = textBoxEquationSystem.Text.Split(new String[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
			if (SystemVerify(t_equationSystem))
			{
				List<char> l_variable = ListeVariable(t_equationSystem);
				int i_nbLigne = NombreLigne(t_equationSystem);
				int i_nbColonne = NombreColonne(t_equationSystem);

				Message("Ligne : " + i_nbLigne.ToString() + " Colonne : " + i_nbColonne.ToString());
				if (i_nbLigne != i_nbColonne)
				{
					Message("Le nombre de ligne doit être égal au nombre de colonne");
					return;
				}

				foreach(char s in l_variable)
				{
					s_matriceX += s.ToString() + " = 0, ";
				}
				s_matriceX = s_matriceX.TrimEnd(new char[] { ',', ' ' });
				textBoxEquationInitiale.Text = s_matriceX;

				Message("Le système d'équation est valide");
			}
			else
			{
				Message("Le système d'équation n'est valide");
			}
        }
	}

	public class GaussSeidel
    {
        private int _i_dimension;
        private int _i_nbIteration;
        private Dictionary<string, float> _d_matriceX;
        private Dictionary<string, float> _d_matriceY;
        private Dictionary<Tuple<int, string>, float> _d_matriceA;
        private Dictionary<int, float> _d_matriceB;
        private List<char> _l_listeVariable;
        private string _s_tempsExecution;
        private Stopwatch _timeExecution;
        private double _i_tolerance; //Tolérance pour savoir si le système est soluble ou non
        public GaussSeidel(Dictionary<Tuple<int, string>, float> pMatriceA, Dictionary<int, float> pMatriceB, Dictionary<string, float> pMatriceX, int pNbIteration, List<char> pListeVariable, double pTolerance)
        {
            _d_matriceA = pMatriceA;
            _d_matriceB = pMatriceB;
            _d_matriceX = pMatriceX;
            _l_listeVariable = pListeVariable;
            _i_dimension = pMatriceB.Count;
            _i_nbIteration = pNbIteration;
            _i_tolerance = pTolerance;

            _d_matriceY = new Dictionary<string, float>();
            _s_tempsExecution = null;
            _timeExecution = new Stopwatch();
        }
        /// <summary>
        /// Cette fonction permet de vérifier si la matrice A est diagonalement dominante
        /// i.e |a(i,i)| >= somme(|a(i,j)|) quand j = 1 a n et j != i
        /// </summary>
        /// <param name="pMatriceA">La matrice a verifier</param>
        /// <returns></returns>
        public bool EstDiagonaleDominant(Dictionary<Tuple<int, string>, float> pMatriceA)
        {
            bool res = true;
            int i_dimension = _i_dimension;
            for(int i = 0; i < i_dimension; i++)
            {
                float sommeLigne = 0;
                float diago = Math.Abs(pMatriceA[Tuple.Create(i, _l_listeVariable[i].ToString())]);
                for(int j = 0; j < i_dimension; j++)
                {
                    if (i == j)
                        continue;
                    sommeLigne += Math.Abs(pMatriceA[Tuple.Create(i, _l_listeVariable[j].ToString())]);
                }
                if (diago <= sommeLigne)
                {
                    res = false;
					break;
				}
            }
            return res;
        }
        /// <summary>
        /// Permet de resoudre le système d'équation
        /// </summary>
        /// <returns></returns>
        public string Resoudre()
        {
            FenetreResultat fenetreResultat = new FenetreResultat();
            fenetreResultat.textBoxResultat.Text += "Starting...\n";
            
            //Timer pour chronometrer le temps d'exécution
            _timeExecution = new Stopwatch();
            _timeExecution.Reset(); //Remet de le chronomètre à zero
            _timeExecution.Start(); //Demarre le chronomètre
            int m = _i_nbIteration; //nb iteration
            bool dominance = EstDiagonaleDominant(_d_matriceA);
            fenetreResultat.textBoxResultat.Text += "Nombre d'itération : " + _i_nbIteration.ToString() + "\n";
            fenetreResultat.textBoxResultat.Text += "Tolérance des erreurs : " + _i_tolerance.ToString() + "\n\n";
            Dictionary<string, double> d_tauxErreur = new Dictionary<string, double>();
			if(dominance)
			{
				//Le système est convergent
				fenetreResultat.textBoxResultat.Text += "Le système est diagonalement dominant. La convergence vers la solution sera effective.\n\n";
			}
			else
			{
				//Le système n'est pas convergent
				fenetreResultat.textBoxResultat.Text += "Le système n'est pas diagonalement dominant. La convergence ne sera pas effective.\nEchanger les équations afin d'obtenir un système convergent.\n\n";
			}
            string sm = null;
            string solution = null;
            while (m > 0)
            {
                //Parcours des itérations
                for(int i = 0; i < _i_dimension; i++)
                {
                    //Parcours des variables du système
                    _d_matriceY[_l_listeVariable[i].ToString()] = (float)(_d_matriceB[i] / _d_matriceA[Tuple.Create(i, _l_listeVariable[i].ToString())]); // x = Y(1)/A(1,1)
                    for (int j = 0; j < _i_dimension; j++)
                    {
                        if (j == i)
                            continue;
                    
                        _d_matriceY[_l_listeVariable[i].ToString()] = _d_matriceY[_l_listeVariable[i].ToString()] - ((_d_matriceA[Tuple.Create(i, _l_listeVariable[j].ToString())] / _d_matriceA[Tuple.Create(i, _l_listeVariable[i].ToString())]) * _d_matriceX[_l_listeVariable[j].ToString()]); // y[i] = y[i] - ((a[i][j] / a[i][i]) * x[j]);
                    }
                    //Calcul du taux d'erreur sur chaque variable
                    d_tauxErreur[_l_listeVariable[i].ToString()] = Math.Abs(_d_matriceY[_l_listeVariable[i].ToString()] - _d_matriceX[_l_listeVariable[i].ToString()]);
                    _d_matriceX[_l_listeVariable[i].ToString()] = _d_matriceY[_l_listeVariable[i].ToString()];
                }
                
                string s_temp;
                s_temp = null;

                foreach (char s in _l_listeVariable)
                {
                    s_temp += s + " = " + _d_matriceY[s.ToString()].ToString() + " , ";
                }
                s_temp = s_temp.TrimEnd(new char[] { ' ', ',', ' ' });
                sm = String.Format("{0}. {1} \n", ((_i_nbIteration + 1) - m).ToString(),s_temp);
                solution = String.Format("{0}", s_temp);
                fenetreResultat.textBoxResultat.Text += sm;

                s_temp = null;

                foreach (char s in _l_listeVariable)
                {
                    s_temp += s + " = " + d_tauxErreur[s.ToString()].ToString() +"\n";
                }
                s_temp = s_temp.TrimEnd(new char[] { ' ', ',', ' ' });
                string staux = String.Format("Taux erreur sur chaque variable : \n{0} \n", s_temp);
                fenetreResultat.textBoxResultat.Text += staux;
                m--;
            }

            bool b_checkError = false;
			double d_valeurTauxErreur = 0;
            foreach (char s in _l_listeVariable)
			{
				d_valeurTauxErreur += d_tauxErreur[s.ToString()];
                if (d_tauxErreur[s.ToString()] > _i_tolerance)
					b_checkError = true;
			}
                
            if(b_checkError == false)
            {
                fenetreResultat.textBoxResultat.Text += "\nLe taux d'erreur global " + d_valeurTauxErreur.ToString() + " est inférieur à la tolérance d'erreur " + _i_tolerance.ToString() + ".\n";
                fenetreResultat.textBoxResultat.Text += "La solution du système est : \n" + solution + "\n";
				solution += " (Taux d'erreur : " + d_valeurTauxErreur + ")";
			}
            else
            {
                fenetreResultat.textBoxResultat.Text += "\nLe taux d'erreur global " + d_valeurTauxErreur.ToString() + " est supérieure à la tolérance d'erreur " + _i_tolerance.ToString() + " .La solution obtenue n'est pas fiable.\n";
				fenetreResultat.textBoxResultat.Text += "La solution du système est : \n" + solution + "\n";
				solution += " (Taux d'erreur : " + d_valeurTauxErreur + ")";
			}
            _timeExecution.Stop();
            fenetreResultat.textBoxResultat.Text += "\nFin de la résolution...\n";
            _s_tempsExecution = "\nTemps d'exécution : " + _timeExecution.ElapsedMilliseconds.ToString() + "ms.";
            fenetreResultat.textBoxResultat.Text += _s_tempsExecution; 
            fenetreResultat.Show();
            
            return solution;
        }

        public string tempsExecution()
        {
            return _s_tempsExecution;
        }
    }
}
