#region

using System;
using System.Collections.Generic;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.Core.Project.Patterns;

#endregion

namespace ConfuserEx.ViewModel
{
    internal interface IRuleContainer
    {
        IList<ProjectRuleVM> Rules
        {
            get;
        }
    }

    public class ProjectRuleVM : ViewModelBase, IViewModel<Rule>
    {
        private readonly Rule rule;
        private string error;
        private PatternExpression exp;

        public ProjectRuleVM(ProjectVM parent, Rule rule)
        {
            Project = parent;
            this.rule = rule;

            var protections = Utils.Wrap(rule, setting => new ProjectSettingVM<Protection>(parent, setting));
            protections.CollectionChanged += (sender, e) => parent.IsModified = true;
            Protections = protections;

            ParseExpression();
        }

        public ProjectVM Project
        {
            get;
        }

        public string Pattern
        {
            get { return rule.Pattern; }
            set
            {
                if(SetProperty(rule.Pattern != value, val => rule.Pattern = val, value, "Pattern"))
                {
                    Project.IsModified = true;
                    ParseExpression();
                }
            }
        }

        public PatternExpression Expression
        {
            get { return exp; }
            set { SetProperty(ref exp, value, "Expression"); }
        }

        public string ExpressionError
        {
            get { return error; }
            set { SetProperty(ref error, value, "ExpressionError"); }
        }

        public ProtectionPreset Preset
        {
            get { return rule.Preset; }
            set
            {
                if(SetProperty(rule.Preset != value, val => rule.Preset = val, value, "Preset"))
                    Project.IsModified = true;
            }
        }

        public bool Inherit
        {
            get { return rule.Inherit; }
            set
            {
                if(SetProperty(rule.Inherit != value, val => rule.Inherit = val, value, "Inherit"))
                    Project.IsModified = true;
            }
        }

        public IList<ProjectSettingVM<Protection>> Protections
        {
            get;
        }

        Rule IViewModel<Rule>.Model => rule;

        private void ParseExpression()
        {
            if(Pattern == null)
                return;
            PatternExpression expression;
            try
            {
                expression = new PatternParser().Parse(Pattern);
                ExpressionError = null;
            }
            catch(Exception e)
            {
                ExpressionError = e.Message;
                expression = null;
            }
            Expression = expression;
        }
    }
}