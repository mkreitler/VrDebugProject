using UnityEngine;
using PalletBuilder;

namespace Assertions {
  public class Assert {
    public static void Test(bool test, string failMessage) {
      if (!test) {
        Switchboard.Broadcast("SetText", "ASSERT: " + failMessage);
        Debug.Log(failMessage);
        Debug.Break();
      }
    }
  }
}
