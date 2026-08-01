using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Backend.Meta.IAP
{
    /// <summary>
    /// Cloud Functions 영수증 검증 호출 구현. 엔드포인트 미설정 시 실패한다.
    /// </summary>
    public sealed class ServerPurchaseValidator : IPurchaseValidator
    {
        private const string ENDPOINT_NOT_CONFIGURED = "Receipt validation endpoint is not configured.";

        private readonly string _validationEndpoint;

        public ServerPurchaseValidator(string validationEndpoint)
        {
            _validationEndpoint = validationEndpoint;
        }

        /// <summary>
        /// 서버에 영수증 검증을 요청한다.
        /// </summary>
        public async UniTask<PurchaseValidationResult> ValidateAsync(PurchaseValidationRequest request)
        {
            if (string.IsNullOrEmpty(_validationEndpoint))
                return PurchaseValidationResult.Failed(ENDPOINT_NOT_CONFIGURED);

            var payload = JsonUtility.ToJson(new ServerValidationPayload
            {
                storeProductId = request.StoreProductId,
                transactionId = request.TransactionId,
                receipt = request.Receipt,
                platform = request.Platform,
            });

            using var webRequest = new UnityWebRequest(_validationEndpoint, UnityWebRequest.kHttpVerbPOST);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    return PurchaseValidationResult.Failed(
                        $"Receipt validation request failed: {webRequest.error}");
                }

                var response = JsonUtility.FromJson<ServerValidationResponse>(
                    webRequest.downloadHandler.text);

                if (response == null || !response.valid)
                {
                    return PurchaseValidationResult.Failed(
                        response?.reason ?? "Receipt validation rejected.");
                }

                DateTimeOffset? expiry = null;
                if (!string.IsNullOrEmpty(response.subscriptionExpiryUtc)
                    && DateTimeOffset.TryParse(
                        response.subscriptionExpiryUtc,
                        out var parsedExpiry))
                {
                    expiry = parsedExpiry.ToUniversalTime();
                }

                return PurchaseValidationResult.Succeeded(expiry);
            }
            catch (Exception exception)
            {
                return PurchaseValidationResult.Failed(exception.Message);
            }
        }

        [Serializable]
        private sealed class ServerValidationPayload
        {
            public string storeProductId;
            public string transactionId;
            public string receipt;
            public string platform;
        }

        [Serializable]
        private sealed class ServerValidationResponse
        {
            public bool valid;
            public string reason;
            public string subscriptionExpiryUtc;
        }
    }
}
