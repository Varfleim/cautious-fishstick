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
            //���� ������ ������� ������ �� �����
            if (fleetsTemplateSummaryPanel != null)
            {
                //���� ������ ���������� ����������� ������
                if (fleetsTemplateSummaryPanel.template == template)
                {
                    //�� ���������� true
                    return true;
                }
            }
            //�����, ���� ������ ������� �������� �� �����
            else if (tFTemplatesTemplateSummaryPanel != null)
            {
                //���� ������ ���������� ����������� ������
                if (tFTemplatesTemplateSummaryPanel.template == template)
                {
                    //�� ���������� true
                    return true;
                }
            }

            return false;
        }
    }
}