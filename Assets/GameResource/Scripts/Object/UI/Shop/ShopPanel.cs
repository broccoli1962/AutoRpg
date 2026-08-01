using System.Collections.Generic;
using Backend.Meta.IAP;
using Backend.Meta.Shop;
using Backend.Meta.Tutorial;
using Backend.Object.Management;
using Backend.Object.UI;
using Backend.Services.Analytics;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Shop
{
    /// <summary>
    /// 상점 메인 패널 View.
    /// </summary>
    public sealed class ShopPanel : UIPanel<ShopPresenter>
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;

        [Header("List")]
        [SerializeField] private RectTransform _productListRoot;
        [SerializeField] private Text _productRowTemplate;

        [Header("Actions")]
        [SerializeField] private CommonButton _restoreButton;
        [SerializeField] private CommonButton _closeButton;

        public RectTransform ProductListRoot => _productListRoot;
        public Text ProductRowTemplate => _productRowTemplate;
        public CommonButton RestoreButton => _restoreButton;
        public CommonButton CloseButton => _closeButton;

        /// <summary>
        /// 제목 텍스트를 설정한다.
        /// </summary>
        public void SetTitle(string text)
        {
            if (_titleText != null)
                _titleText.text = text;
        }

        /// <summary>
        /// 상품 목록 행을 갱신한다.
        /// </summary>
        public void SetProductRows(IReadOnlyList<ShopProductRowViewModel> rows)
        {
            if (_productListRoot == null || _productRowTemplate == null)
                return;

            for (var i = _productListRoot.childCount - 1; i >= 0; i--)
            {
                var child = _productListRoot.GetChild(i);
                if (child.GetComponent<Text>() != _productRowTemplate)
                    Destroy(child.gameObject);
            }

            _productRowTemplate.gameObject.SetActive(false);

            if (rows == null)
                return;

            foreach (var row in rows)
            {
                var rowObject = Instantiate(_productRowTemplate.gameObject, _productListRoot);
                rowObject.SetActive(true);

                var label = rowObject.GetComponent<Text>();
                if (label == null)
                    continue;

                label.text = row.Label;
            }
        }
    }

    /// <summary>
    /// 상점 상품 행 ViewModel.
    /// </summary>
    public readonly struct ShopProductRowViewModel
    {
        public string ProductId { get; }
        public string Label { get; }
        public bool CanPurchase { get; }

        public ShopProductRowViewModel(string productId, string label, bool canPurchase)
        {
            ProductId = productId;
            Label = label;
            CanPurchase = canPurchase;
        }
    }

    /// <summary>
    /// 상점 패널 Presenter.
    /// </summary>
    public sealed class ShopPresenter : UIPresenter<ShopPanel>
    {
        private readonly List<ShopProductRowViewModel> _rows = new();

        public override void OnOpen()
        {
            if (IsBlockedByTutorial())
            {
                UIManager.Close(View);
                return;
            }

            BackendAnalyticsEvents.ReportShopView();
            RefreshLabels();
            RefreshProducts();
            BindButtons();
            LocalizeTable.OnChangedLanguage += RefreshLabels;
        }

        public override void OnClose()
        {
            LocalizeTable.OnChangedLanguage -= RefreshLabels;
        }

        private void BindButtons()
        {
            Bind(View.RestoreButton, RestorePurchasesAsync);
            Bind(View.CloseButton, CloseSelf);
        }

        private void Bind(CommonButton button, System.Action handler)
        {
            if (button == null || handler == null)
                return;

            button.OnClickAsObservable()
                .Subscribe(_ => handler())
                .AddTo(View);
        }

        private void RefreshLabels()
        {
            View.SetTitle("shop.title".GetLocalizeText());
            RefreshProducts();
        }

        private void RefreshProducts()
        {
            _rows.Clear();
            var catalog = ShopCatalogTableProvider.Get();
            var shopService = ResolveShopService();

            if (catalog?.Products == null)
            {
                View.SetProductRows(_rows);
                return;
            }

            foreach (var product in catalog.Products)
            {
                if (product == null)
                    continue;

                var displayName = product.DisplayNameKey.GetLocalizeText();
                if (string.IsNullOrEmpty(displayName))
                    displayName = product.ProductId;

                var priceLabel = product.PriceKrw > 0
                    ? $"₩{product.PriceKrw:N0}"
                    : string.Empty;

                var canPurchase = shopService == null || shopService.CanPurchase(product);
                var statusSuffix = canPurchase ? string.Empty : " (sold out)";

                _rows.Add(new ShopProductRowViewModel(
                    product.ProductId,
                    $"{displayName} {priceLabel}{statusSuffix}".Trim(),
                    canPurchase));
            }

            View.SetProductRows(_rows);
        }

        private static bool IsBlockedByTutorial()
        {
            var gate = TutorialManager.TryGetGate();
            return gate != null && gate.IsTutorialActive;
        }

        private static ShopService ResolveShopService()
        {
            return IapManager.TryGetShop();
        }

        private void RestorePurchasesAsync()
        {
            IapManager.RestorePurchasesAsync().Forget();
        }

        private void CloseSelf()
        {
            UIManager.Close(View);
        }
    }
}
