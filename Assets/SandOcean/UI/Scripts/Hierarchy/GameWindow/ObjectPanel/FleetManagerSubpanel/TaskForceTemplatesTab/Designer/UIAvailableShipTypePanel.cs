namespace SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates.Designer
{
    public class UIAvailableShipTypePanel : UIAShipTypePanel
    {
        public UICurrentShipTypePanel siblingCurrentShipTypePanel;

        public void AddShipType()
        {
            //����������� ���������� ���������� ���� �������
            parentTaskForceTemplateDesignerSubtab.AddAvailableShipType(this);
        }
    }
}