using System.Security.Cryptography;

namespace ecopack.Api.Support
{
    /// <summary>
    /// 프로젝트 번호·대화 세션 ID·저장 파일명처럼 서버가 채번하는 값을 만든다.
    /// 예전에는 시각(밀리초)만으로 만들어서 같은 순간에 들어온 요청끼리 번호가 겹칠 수 있었다.
    /// 프로젝트 번호는 기존 17자리 형식을 유지하면서 겹치지만 않게 하고(NewTimestampId),
    /// 대화 세션 ID 와 저장 파일명은 난수를 넣어 짐작도 할 수 없게 한다.
    /// </summary>
    public static class IdGenerator
    {
        /// <summary>
        /// 시각(17자리) + 난수(16진수 8자리). 대화 세션 ID 에 쓴다. 예: 20261014093015123A1B2C3D
        /// 앞부분이 시각이라 번호순으로 정렬하면 만든 순서가 된다.
        /// </summary>
        public static string NewTimeOrderedId() =>
            $"{DateTime.Now:yyyyMMddHHmmssfff}{RandomNumberGenerator.GetHexString(8)}";

        private static readonly object TimestampLock = new();
        private static DateTime _lastTimestamp = DateTime.MinValue;

        /// <summary>
        /// 시각 17자리(yyyyMMddHHmmssfff). 프로젝트 번호에 쓴다. 예: 20261014093015123
        /// 같은 밀리초에 여러 번 불려도 겹치지 않게, 직전에 내준 값보다 항상 1밀리초 이상 뒤의 값을 준다.
        /// (한 서버 안에서만 보장된다. 서버가 여러 대면 호출부에서 DB 중복 여부도 확인해야 한다)
        /// </summary>
        public static string NewTimestampId()
        {
            lock (TimestampLock)
            {
                var now = DateTime.Now;
                // 밀리초 아래는 번호에 안 들어가므로 잘라내고 비교한다
                now = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerMillisecond, now.Kind);
                if (now <= _lastTimestamp)
                {
                    now = _lastTimestamp.AddMilliseconds(1);
                }
                _lastTimestamp = now;
                return now.ToString("yyyyMMddHHmmssfff");
            }
        }

        /// <summary>저장 파일명 등에 쓰는, 짐작할 수 없는 고유 값(16진수 32자리).</summary>
        public static string NewRandomId() => Guid.NewGuid().ToString("N");
    }
}
