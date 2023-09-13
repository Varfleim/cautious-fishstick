using SandOcean.UI.GameWindow.Object.FleetManager.Fleets;
using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.Warfare.Fleet
{
    public struct CTFTemplateDisplayedSummaryPanel
    {
        public UITFTemplateSummaryPanel fleetsTemplateSummaryPanel;

        public UI.GameWindow.Object.FleetManager.TaskForceTemplates.List.UITFTemplateSummaryPanel tFTemplatesTemplateSummaryPanel;

        public bool CheckTemplate(
            DTFTemplate template)
        {
            //Если панель вкладки флотов не пуста
            if (fleetsTemplateSummaryPanel != null)
            {
                //Если панель отображает запрошенный шаблон
                if (fleetsTemplateSummaryPanel.template == template)
                {
                    //То возвращаем true
                    return true;
                }
            }
            //Иначе, если панель вкладки шаблонов не пуста
            else if (tFTemplatesTemplateSummaryPanel != null)
            {
                //Если панель отображает запрошенный шаблон
                if (tFTemplatesTemplateSummaryPanel.template == template)
                {
                    //То возвращаем true
                    return true;
                }
            }

            return false;
        }
    }
}