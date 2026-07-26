using AudioMetadataManager.UI.Views.Models.Simulation;
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

namespace AudioMetadataManager.UI.Views
{
    /// <summary>
    /// Lógica de interacción para AudioFileDetailsView.xaml
    /// </summary>
    public partial class AudioFileDetailsView : UserControl
    {

        /// <summary>
        /// Se produce cuando la vista de simulación solicita validar
        /// los cambios aprobados.
        /// </summary>
        public event EventHandler?
            ValidateApprovedChangesRequested
        {
            add =>
                SimulationPlanViewControl
                    .ValidateApprovedChangesRequested += value;

            remove =>
                SimulationPlanViewControl
                    .ValidateApprovedChangesRequested -= value;
        }
        public AudioFileDetailsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Plan visual mostrado en la pestaña de simulación.
        /// </summary>
        public SimulationPlanViewModel? SimulationPlan
        {
            get =>
                SimulationPlanViewControl.DataContext
                as SimulationPlanViewModel;

            set =>
                SimulationPlanViewControl.DataContext =
                    value;
        }
    }
}
