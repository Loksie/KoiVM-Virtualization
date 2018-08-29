#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Confuser.Core;
using Confuser.Core.Project;

#endregion

namespace ConfuserEx.ViewModel
{
    public class ProjectVM : ViewModelBase, IViewModel<ConfuserProject>, IRuleContainer
    {
        private bool modified;
        private ProjectSettingVM<Packer> packer;

        public ProjectVM(ConfuserProject proj, string fileName)
        {
            Project = proj;
            FileName = fileName;

            var modules = Utils.Wrap(proj, module => new ProjectModuleVM(this, module));
            modules.CollectionChanged += (sender, e) => IsModified = true;
            Modules = modules;

            var plugins = Utils.Wrap(proj.PluginPaths, path => new StringItem(path));
            plugins.CollectionChanged += (sender, e) => IsModified = true;
            Plugins = plugins;

            var probePaths = Utils.Wrap(proj.ProbePaths, path => new StringItem(path));
            probePaths.CollectionChanged += (sender, e) => IsModified = true;
            ProbePaths = probePaths;

            var rules = Utils.Wrap(proj.Rules, rule => new ProjectRuleVM(this, rule));
            rules.CollectionChanged += (sender, e) => IsModified = true;
            Rules = rules;

            Protections = new ObservableCollection<ConfuserComponent>();
            Packers = new ObservableCollection<ConfuserComponent>();
            ComponentDiscovery.LoadComponents(Protections, Packers, Assembly.Load("Confuser.Protections").Location);
            ComponentDiscovery.LoadComponents(Protections, Packers, Assembly.Load("Confuser.Renamer").Location);
        }

        public ConfuserProject Project
        {
            get;
        }

        public bool IsModified
        {
            get { return modified; }
            set { SetProperty(ref modified, value, "IsModified"); }
        }

        public string Seed
        {
            get { return Project.Seed; }
            set { SetProperty(Project.Seed != value, val => Project.Seed = val, value, "Seed"); }
        }

        public bool Debug
        {
            get { return Project.Debug; }
            set { SetProperty(Project.Debug != value, val => Project.Debug = val, value, "Debug"); }
        }

        public string BaseDirectory
        {
            get { return Project.BaseDirectory; }
            set { SetProperty(Project.BaseDirectory != value, val => Project.BaseDirectory = val, value, "BaseDirectory"); }
        }

        public string OutputDirectory
        {
            get { return Project.OutputDirectory; }
            set { SetProperty(Project.OutputDirectory != value, val => Project.OutputDirectory = val, value, "OutputDirectory"); }
        }

        public ProjectSettingVM<Packer> Packer
        {
            get
            {
                if(Project.Packer == null)
                    packer = null;
                else
                    packer = new ProjectSettingVM<Packer>(this, Project.Packer);
                return packer;
            }
            set
            {
                var vm = (IViewModel<SettingItem<Packer>>) value;
                var changed = vm == null && Project.Packer != null || vm != null && Project.Packer != vm.Model;
                SetProperty(changed, val => Project.Packer = val == null ? null : val.Model, vm, "Packer");
            }
        }

        public IList<ProjectModuleVM> Modules
        {
            get;
        }

        public IList<StringItem> Plugins
        {
            get;
        }

        public IList<StringItem> ProbePaths
        {
            get;
        }

        public ObservableCollection<ConfuserComponent> Protections
        {
            get;
        }

        public ObservableCollection<ConfuserComponent> Packers
        {
            get;
        }

        public string FileName
        {
            get;
            set;
        }

        public IList<ProjectRuleVM> Rules
        {
            get;
        }

        ConfuserProject IViewModel<ConfuserProject>.Model => Project;

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            if(property != "IsModified")
                IsModified = true;
        }
    }
}