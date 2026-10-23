/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - LoginGuard (로그인 시도 제한)
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 같은 아이디로 비밀번호를 연달아 틀리면 그 아이디를 잠시 잠근다.
 *      비밀번호를 하나씩 넣어 보는 공격을 느리게 만들기 위해서다.
 *    - 기본값: 5번 연속 실패하면 5분 잠금. (LoginGuard:MaxFailures / LockMinutes 설정으로 바꿀 수 있다)
 *
 * 2. 알아둘 점
 *    - 기록은 서버 메모리에만 있다. 서버를 다시 켜면 풀린다.
 *      재시작 후에도 유지하려면 DB 컬럼이 필요하다(보고서 5단계 범위).
 *    - 없는 아이디도 똑같이 센다. 잠금 여부로 아이디가 있는지 알아낼 수 없게 하기 위해서다.
 *    - 남의 아이디로 일부러 틀려서 잠그는 것은 막지 못한다. 그래서 잠금 시간을 짧게 둔다.
 * ==============================================================================
 */
using System.Collections.Concurrent;

namespace ecopack.Api.Support
{
    public class LoginGuard
    {
        private sealed class Entry
        {
            public int Failures;
            public DateTime LastFailure;
            public DateTime LockedUntil;
        }

        private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
        private readonly int _maxFailures;
        private readonly TimeSpan _lockTime;

        public LoginGuard(IConfiguration config)
        {
            _maxFailures = Math.Max(1, config.GetValue("LoginGuard:MaxFailures", 5));
            _lockTime = TimeSpan.FromMinutes(Math.Max(1, config.GetValue("LoginGuard:LockMinutes", 5)));
        }

        /// <summary>지금 잠겨 있으면 남은 시간을 돌려준다. 잠겨 있지 않으면 null.</summary>
        public TimeSpan? LockedFor(string id)
        {
            if (_entries.TryGetValue(Key(id), out var e))
            {
                lock (e)
                {
                    var left = e.LockedUntil - DateTime.Now;
                    if (left > TimeSpan.Zero) return left;
                }
            }
            return null;
        }

        /// <summary>실패를 한 번 센다. 이번 실패로 잠겼으면 true.</summary>
        public bool RecordFailure(string id)
        {
            Prune();
            var e = _entries.GetOrAdd(Key(id), _ => new Entry());
            lock (e)
            {
                var now = DateTime.Now;
                // 마지막 실패에서 잠금 시간만큼 지났으면 처음부터 다시 센다
                if (now - e.LastFailure > _lockTime) e.Failures = 0;

                e.Failures++;
                e.LastFailure = now;
                if (e.Failures >= _maxFailures)
                {
                    e.LockedUntil = now + _lockTime;
                    e.Failures = 0;
                    return true;
                }
                return false;
            }
        }

        /// <summary>로그인에 성공하면 그 아이디의 실패 기록을 지운다.</summary>
        public void RecordSuccess(string id) => _entries.TryRemove(Key(id), out _);

        private static string Key(string id) => id.Trim();

        /// <summary>아무 아이디나 넣어 메모리를 채우지 못하게, 오래된 기록을 가끔 치운다.</summary>
        private void Prune()
        {
            if (_entries.Count < 10_000) return;
            var cutoff = DateTime.Now - _lockTime;
            foreach (var kv in _entries)
            {
                if (kv.Value.LastFailure < cutoff && kv.Value.LockedUntil < DateTime.Now)
                {
                    _entries.TryRemove(kv.Key, out _);
                }
            }
        }
    }
}
