using System.Threading.Tasks;
using UnityEngine;

public class CommunicationServicesSample : MonoBehaviour
{
    private CommunicationServices communicationServices;

    // アクセストークン
    private string user_token_ = "";

    // 接続先
    private string call_text = "";

    // 表示名
    private string user_name = "ACS Unity User";

    // Start is called before the first frame update
    async void Start()
    {
        communicationServices = new CommunicationServices();
        await communicationServices.Init(user_token_, user_name);

        await Task.Delay(1000);

        await communicationServices.CallButton_ClickAsync(call_text);
    }
}
