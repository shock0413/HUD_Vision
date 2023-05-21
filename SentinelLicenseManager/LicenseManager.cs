using Aladdin.HASP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SentinelLicenseManager
{
    public class LicenseManager
    {
        public const string vendorCode =
        "9AjfNa8DQriYB49kxYmAWrJLBV70i0jlbN/BrhRnoRRA/W70a3dvKogR8bR6T1MpqN7o4N3Z4ZBNnEw+" +
        "Zt9Wy6h00WSiq3LU0K7rZKLUeB5XeTcwqmnuQGiejLPQUvPLRJvI2zLNidwCJGrJ8Dy3wRaVyFmQbkRS" +
        "3ah6lkGlvBahIBENTtuY5yJH4uHiQffUOxxLA1LHoAT1V8+cnZm/zPctKhWzJoH6zB8a/cQzLkKQbnDZ" +
        "Wuql2oLM9u8gIwXFQA5JzQcxIOC+rB5LQOU07PglcY/9KQxeSDmMB58xKfre+XnRXoGlNzSYt96Qdw01" +
        "mbzbjpyHDAFUNrjh1Un6tS00WLMVCZGTjIaIiMfdGaQv+uiy9w+Wo8qyWEZ3NPoETh+lYDbemYNJtjs1" +
        "G6DiP9GIabWYdKgkU7Mj3GfepP6HN9GGaNJRlNmNcNhsFmPEBx2zKybaHuSHaJzS3GqmTE8EBMuEMYo3" +
        "UlBkVVt2M8pQXoVVugVvfcAia7WPFlXJBsqocclv2vB6hQ9qM05ejwzpWLJV5/75t/vV0AgkX/ePEq7W" +
        "8h5dZmVflhcw+a1KW3bdeIyVWFxT23A4qtVo0EcPZm2dD1cnwFNKMZSYdh6u09tuMREASzFRwT3uHQd0" +
        "ybbkZPwWoI6phs7wc1pQ22XjdoUyDFgY2lD1B5pshBhmZUuXwRlEGt6bZ1+iRlmjClEKtbIlJwtw/Dtu" +
        "tuWVvDrlsEQagpuNdsa6ic9QbJQDB9EVJe8x5Uf70rcnolm5XF6QjqpR86g0dmW3dx+57kBCsMTm6HdO" +
        "HmChLxZ7+WtXWOKQAazwaUwiKTR81xSbvzxI6ocXwIFPreqwpArvbwgWKrZ4/70TtKr/qqLfMgI75puI" +
        "6e8lHeOPz5LJjFHhWppIAhi8eR/LVo6BSBXWUorFGnxWWYtD/TWG3+4/KmqFp4FotHBc1p+MHoaJHU90" +
        "JQtQrGhugFvsHo2J7TDl/Q==";

        Thread thread;

        int interval = 1;

        bool stopFlag = false;

        public delegate void OnLicenseErrorEventHandler();
        public event OnLicenseErrorEventHandler OnLicenseErrorEvent = delegate { };

        public bool CurrentState { get; set; }

        public LicenseManager()
        {

        }

        /// <summary>
        /// 비동기로 라이센스 체크 실시
        /// </summary>
        /// <param name="interval">체크 간격 초</param>
        public void StartCheckAsync(int interval)
        {
            if(thread == null || !thread.IsAlive)
            {
                this.interval = interval;

                thread = new Thread(new ThreadStart(() =>
                {
                    HaspFeature feature = HaspFeature.Default;

                    Hasp hasp = new Hasp(feature);
                    HaspStatus status = hasp.Login(vendorCode);
                    if (HaspStatus.StatusOk != status)
                    {
                        OnLicenseErrorEvent();
                    }

                    while (!stopFlag)
                    {
                        string info = null;
                        status = hasp.GetSessionInfo(Hasp.SessionInfo, ref info);

                        if (HaspStatus.StatusOk != status)
                        {
                            OnLicenseErrorEvent();
                            CurrentState = false;
                        }
                        else
                        {
                            CurrentState = true;
                        }

                        Thread.Sleep(this.interval);
                    }
                    hasp.Logout();
                }));

                thread.Start();
            }
        }

        public void StopCheck()
        {
            stopFlag = true;
        }
    }
}
