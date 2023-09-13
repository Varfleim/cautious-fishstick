namespace SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates.Designer
{
    public class UIAvailableShipTypePanel : UIAShipTypePanel
    {
        public UICurrentShipTypePanel siblingCurrentShipTypePanel;

        public void AddShipType()
        {
            //Запрашиваем добавление доступного типа корабля
            parentTaskForceTemplateDesignerSubtab.AddAvailableShipType(this);
        }
    }
}