using AmetisAPI.Core.Push;

namespace AmetisAPI.Core.Tests.Push
{
    public class PushEndpointRulesTests
    {
        [Theory]
        [InlineData("https://fcm.googleapis.com/fcm/send/abc", true)]
        [InlineData("https://updates.push.services.mozilla.com/wpush/v2/abc", true)]
        [InlineData("https://wns2-par02p.notify.windows.com/w/?token=abc", true)]
        [InlineData("https://web.push.apple.com/abc", true)]
        [InlineData("http://fcm.googleapis.com/fcm/send/abc", false)]
        [InlineData("https://fcm.googleapis.com:8443/fcm/send/abc", false)]
        [InlineData("https://localhost/push", false)]
        [InlineData("https://evil-fcm.googleapis.com.attacker.test/x", false)]
        [InlineData("https://notfcm.googleapis.com/x", false)]
        [InlineData("pas une adresse", false)]
        public void Only_Known_Push_Services_Over_Https_Are_Accepted(string p_Endpoint, bool p_Expected)
        {
            Check.That(PushEndpointRules.IsAllowed(p_Endpoint)).IsEqualTo(p_Expected);
        }
    }
}
