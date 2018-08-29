#region

using System.Collections.Generic;
using System.Threading;
using Confuser.Core.Project;

#endregion

namespace ConfuserEx.ViewModel
{
    public class ProjectModuleVM : ViewModelBase, IViewModel<ProjectModule>, IRuleContainer
    {
        private readonly ProjectVM parent;
        private string asmName = "Unknown";
        private bool isSelected;
        private string simpleName;

        public ProjectModuleVM(ProjectVM parent, ProjectModule module)
        {
            this.parent = parent;
            Module = module;

            var rules = Utils.Wrap(module.Rules, rule => new ProjectRuleVM(parent, rule));
            rules.CollectionChanged += (sender, e) => parent.IsModified = true;
            Rules = rules;

            if(module.Path != null)
            {
                SimpleName = System.IO.Path.GetFileName(module.Path);
                LoadAssemblyName();
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value, "IsSelected"); }
        }

        public ProjectModule Module
        {
            get;
        }

        public string Path
        {
            get { return Module.Path; }
            set
            {
                if(SetProperty(Module.Path != value, val => Module.Path = val, value, "Path"))
                {
                    parent.IsModified = true;
                    SimpleName = System.IO.Path.GetFileName(Module.Path);
                    LoadAssemblyName();
                }
            }
        }

        public string SimpleName
        {
            get { return simpleName; }
            private set { SetProperty(ref simpleName, value, "SimpleName"); }
        }

        public string AssemblyName
        {
            get { return asmName; }
            private set { SetProperty(ref asmName, value, "AssemblyName"); }
        }

        public string SNKeyPath
        {
            get { return Module.SNKeyPath; }
            set
            {
                if(SetProperty(Module.SNKeyPath != value, val => Module.SNKeyPath = val, value, "SNKeyPath"))
                    parent.IsModified = true;
            }
        }

        public string SNKeyPassword
        {
            get { return Module.SNKeyPassword; }
            set
            {
                if(SetProperty(Module.SNKeyPassword != value, val => Module.SNKeyPassword = val, value, "SNKeyPassword"))
                    parent.IsModified = true;
            }
        }

        public IList<ProjectRuleVM> Rules
        {
            get;
        }

        ProjectModule IViewModel<ProjectModule>.Model => Module;

        private void LoadAssemblyName()
        {
            AssemblyName = "Loading...";
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var path = System.IO.Path.Combine(parent.BaseDirectory, Path);
                    if(!string.IsNullOrEmpty(parent.FileName))
                        path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(parent.FileName), path);
                    var name = System.Reflection.AssemblyName.GetAssemblyName(path);
                    AssemblyName = name.FullName;
                }
                catch
                {
                    AssemblyName = "Unknown";
                }
            });
        }
    }
}