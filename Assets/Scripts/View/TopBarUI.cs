using UnityEngine;
using UnityEngine.UI;
using AmericanOfficialNotes.Controller;

namespace AmericanOfficialNotes.View
{
    public class TopBarUI : MonoBehaviour
    {
        [SerializeField] private Text _fundsText;
        [SerializeField] private Text _dayText;
        [SerializeField] private Text _popText;

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            UpdateFunds(gm.GetFunds());
            UpdateDay(gm.TimeController.GetCurrentDay());
            gm.OnFundsChanged += UpdateFunds;
            gm.OnDayAdvanced += UpdateDay;
            gm.OnParametersChanged += UpdatePopulation;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnFundsChanged -= UpdateFunds;
            GameManager.Instance.OnDayAdvanced -= UpdateDay;
            GameManager.Instance.OnParametersChanged -= UpdatePopulation;
        }

        private void UpdateFunds(decimal funds)
        {
            if (_fundsText != null) _fundsText.text = $"资金: ${funds:N0}";
        }

        private void UpdateDay(int day)
        {
            if (_dayText != null) _dayText.text = $"第 {day} 天";
            UpdatePopulation();
        }

        private void UpdatePopulation()
        {
            var gm = GameManager.Instance;
            if (gm == null || _popText == null) return;
            int total = gm.PopulationController.GetTotalPopulation();
            int employed = gm.PopulationController.GetEmployedPopulation();
            _popText.text = $"人口: {total} (就业: {employed})";
        }
    }
}
