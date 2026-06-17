using UnityEngine;
using UnityEngine.UI;

namespace DawnOfShadow.MVP.Shop
{
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private Button buttonX;

        private void Start()
        {
            if (buttonX != null)
            {
                buttonX.onClick.AddListener(() => gameObject.SetActive(false));
            }
        }
    }
}
