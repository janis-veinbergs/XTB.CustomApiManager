﻿using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Windows.Forms;
using xrmtb.XrmToolBox.Controls.Controls;
using XrmToolBox.Extensibility;
using XTB.CustomApiManager.Entities;
using XTB.CustomApiManager.Helpers;
using XTB.CustomApiManager.Proxy;
//using static XTB.CustomApiManager.Entities.CustomAPI;

namespace XTB.CustomApiManager
{
    public partial class NewCustomApiForm : Form
    {
        #region Private Fields

        //private Control focus;
        private IOrganizationService _service;
        

        #endregion Private Fields

        #region Public Constructors

        public NewCustomApiForm(IOrganizationService service,Entity publisher)
        {
            InitializeComponent();
            _service = service;
            

            dlgLookupPublisher.Service = service;
            dlgLookupPluginType.Service = service;
            cdsCboPrivileges.OrganizationService = service;
            cdsCboPrivileges.DataSource = 

            cboEntities.Service = service;
            cboEntities.Update();

            cboBindingType.DataSource = Enum.GetValues(typeof(CustomAPI.BindingType_OptionSet));           
            cboAllowedCustomProcessingStep.DataSource = Enum.GetValues(typeof(CustomAPI.AllowedCustomProcessingStepType_OptionSet));

            if (publisher != null)
            {
                txtLookupPublisher.Entity = publisher;
                txtLookupPublisher.Text = publisher.Attributes[Publisher.PrimaryName].ToString();
                txtPrefix.Text = publisher.Attributes[Publisher.Prefix].ToString();
            }


            LoadPrivileges();

            //default values
            cboBindingType.SelectedIndex = (int)CustomAPI.BindingType_OptionSet.Global;
            cboAllowedCustomProcessingStep.SelectedIndex = (int)CustomAPI.AllowedCustomProcessingStepType_OptionSet.None;


        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// GUID of the Custom API Created
        /// </summary>
        public Guid NewCustomApiId { get; private set; }

        #endregion Public Properties



        #region Private Event Handlers
        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                //todo modify for Update

                NewCustomApiId = _service.Create(CustomApiToCreate());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                DialogResult = DialogResult.None;
            }

        }


        private void txtUniqueName_Leave(object sender, EventArgs e)
        {
            if (txtName.Text == string.Empty)
            {
                txtName.Text = txtUniqueName.Text;
            }

            if (txtDisplayName.Text == string.Empty)
            {
                txtDisplayName.Text = txtUniqueName.Text;
            }

        }

        private void cboBindingType_SelectedIndexChanged(object sender, EventArgs e)
        {

            cboEntities.Enabled = cboBindingType.SelectedIndex == (int)CustomAPI.BindingType_OptionSet.Entity;
            if (cboEntities.Enabled == true)
            {
                cboEntities.LoadData();
            }
            else
            {
                cboEntities.ClearData();
            }
        }

        #endregion Private Event Handlers

        #region Private Methods



        #endregion Private Methods

        private void btnLookupPluginType_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            switch (dlgLookupPluginType.ShowDialog(this))
            {
                case DialogResult.OK:
                    txtLookupPluginType.Entity = dlgLookupPluginType.Entity;

                    break;
                case DialogResult.Abort:
                    //txtLookupPluginType.Entity = null;
                    break;
            }
            //cmbValue.Text = (txtLookup?.Entity?.Id ?? Guid.Empty).ToString();
            Cursor = Cursors.Default;
        }

        private void btnLookupPublisher_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            switch (dlgLookupPublisher.ShowDialog(this))
            {
                case DialogResult.OK:
                    txtLookupPublisher.Entity = dlgLookupPublisher.Entity;
                    var prefix = _service.GetPublisherPrefix((Guid)dlgLookupPublisher.Entity.Attributes[Publisher.PrimaryKey]);
                    txtPrefix.Text = $"{prefix}_";

                    //unlock

                    break;
                case DialogResult.Abort:
                    
                    break;
            }
           
            Cursor = Cursors.Default;
        }



        private Entity CustomApiToCreate() 
        {
            var api = new Entity(CustomAPI.EntityName);

            api[CustomAPI.UniqueName] = txtPrefix.Text + txtUniqueName.Text;  

            api[CustomAPI.AllowedCustomProcessingStepType] = new OptionSetValue(cboAllowedCustomProcessingStep.SelectedIndex);  
            api[CustomAPI.BindingType] = new OptionSetValue(cboBindingType.SelectedIndex); 
            api[CustomAPI.Description] = txtDescription.Text;
            api[CustomAPI.DisplayName] = txtDisplayName.Text;
            api[CustomAPI.ExecutePrivilegeName] = cdsCboPrivileges.Text;
            api[CustomAPI.IsFunction] = chkIsFunction.Checked;
            api[CustomAPI.IsPrivate] = chkIsPrivate.Checked;
            api[CustomAPI.PrimaryName] = txtName.Text;

            if (IsBoundToEntity()) 
            {
                api[CustomAPI.BoundEntityLogicalName] = cboEntities.SelectedEntity?.LogicalName;
            }

            if (!string.IsNullOrEmpty(txtLookupPluginType.Text)) 
            {
                api[CustomAPI.PluginType] = new EntityReference(Plug_inType.EntityName, txtLookupPluginType.Id);
            }


            return api;
        }

        

        
        private bool IsBoundToEntity()
        {
            return cboBindingType.SelectedIndex == (int)CustomAPI.BindingType_OptionSet.Entity
                    ||
                    cboBindingType.SelectedIndex == (int)CustomAPI.BindingType_OptionSet.EntityCollection;
        }

        private bool IsPublisherSelected()
        {
            return !string.IsNullOrEmpty(txtLookupPublisher.Text);
        }

        private bool CanCreate() 
        {
            return !string.IsNullOrEmpty(txtPrefix.Text) &&
                    !string.IsNullOrEmpty(txtUniqueName.Text) &&
                    !string.IsNullOrEmpty(txtName.Text) &&
                    !string.IsNullOrEmpty(txtDisplayName.Text) &&
                    (IsBoundToEntity() && !string.IsNullOrEmpty(cboEntities.SelectedEntity?.LogicalName)
                      ||
                    !IsBoundToEntity());


        }

        

        

       

        private void LoadPrivileges()
        {
            
             var privileges = _service.GetPrivileges();


            cdsCboPrivileges.DataSource = privileges;
            cdsCboPrivileges.SelectedIndex = -1;

        }

        
    }
}