using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using WebShared.Services;
using WebShared.Models;



namespace WebClient.Sections
{
    public partial class OtpPage : ComponentBase
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        public bool loadProses { get; set; } = false;
        public bool auth { get; set; } = false;
        public bool succesretry { get; set; } = false;

        public bool prosesOtp { get; set; } = false;

        [Parameter]
        public string? UId { get; set; }

        public string OTP { get; set; } = "";

        [Parameter]
        public int? IdInfo { get; set; } = 0;

        public bool IdSucces { get; set; } = false;

        [Parameter]
        public string? NoTlp { get; set; }

        [Parameter]
        public string? Pass { get; set; }

        public string? TokenJwt { get; set; }

        private async Task LoginStart()
        {
            LoginViewModel Model = new();

            var param = new ParameterApi
            {
                routeApi = "auth",
                prosesApi = "login",
                ParamUrl = new ParameterApi.ParameterUrl { }
            };

            Model.NoTlp = NoTlp;
            Model.Password = Pass;

            var loginStatus = await NewPdjService.CheckLoginApi(param, Model);

            if (loginStatus)
            {
                auth = false;
                StateHasChanged();

                var dataDecodeA = await pdjService.DecodeEncode(true, new List<string> { ScopedDataServices?.ScopedData?.DataLogin?.No_Id });
                NavigationManager.NavigateTo($"/auth-user-session/session/{dataDecodeA[0]}", forceLoad: true);
            } 
            else
            {
                auth = true;
                StateHasChanged();
                return;
            }


        }

        public async Task cobaLagi()
        {
            if (IdInfo == 1)
            {
                object dataobj = new
                {
                    No_Id = ScopedDataServices.ScopedData.DataLogin.No_Id,
                    New_Token = TokenJwt,
                    Password = Pass
                };

                var info = await NewPdjService.ChangeAkunApi(new ParameterApi
                {
                    routeApi = "auth",
                    prosesApi = "change-phone",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                }, dataobj);

                IdSucces = info;
                return;
            }

            else if (IdInfo == 2)
            {
                object dataobj = new
                {
                    Token = TokenJwt,
                    New_Password = Pass,
                };

                var info = await NewPdjService.ChangeAkunApi(new ParameterApi
                {
                    routeApi = "auth",
                    prosesApi = "change-password",
                }, dataobj);

                IdSucces = info;
                return;
            }
        }

        public async Task reTryOtp()
        {
            var otp = await NewPdjService.SendOtp(new ParameterApi
            {
                routeApi = "auth",
                prosesApi = "request-otp",
                ParamUrl = new ParameterApi.ParameterUrl()
            }, new { Telepon = NoTlp, Check_User = false });

            succesretry = true;
            StateHasChanged();

            await Task.Delay(15000);

            succesretry = false;
            StateHasChanged();
        }

        public async Task sendOtp()
        {
            Console.WriteLine($"{OTP} {IdInfo}");
            if (OTP != null)
            {
                if (IdInfo == 1)
                {
                    object dataobj = new
                    {
                        No_Id = ScopedDataServices.ScopedData.DataLogin.No_Id,
                        Telepon = NoTlp,
                        OTP = OTP,
                        Password = Pass
                    };

                    var info = await NewPdjService.ChangeAkunApi(new ParameterApi
                    {
                        routeApi = "auth",
                        prosesApi = "change-phone",
                    }, dataobj);

                    if (info)
                    {
                        await NewPdjService.RefreshDataAuthKonten();
                        prosesOtp = true;
                        StateHasChanged();
                    }

                    IdSucces = info;
                    return;
                }
            
                else if ( IdInfo == 2 )
                {
                    object dataobj = new
                    {
                        Telepon = NoTlp,
                        OTP = OTP,
                        New_Password = Pass,
                    };

                    var info = await NewPdjService.ChangeAkunApi(new ParameterApi
                    {
                        routeApi = "auth",
                        prosesApi = "change-password",
                    }, dataobj);


                    if (info)
                    {
                        prosesOtp = true;
                        StateHasChanged();
                    }

                    IdSucces = info;
                    return;
                }




                var data = await pdjService.RegistUserApi(new ParameterApi
                {
                    routeApi = "auth",
                    prosesApi = "register",
                }, 
                new DataLogin 
                { 
                    Telepon = NoTlp , 
                    tokenJwt = OTP.ToString(), 
                    Password = Pass 
                });

                if (data)
                {
                    prosesOtp = true;
                    StateHasChanged();

                    await Task.Delay(3000);
                    await LoginStart();

                    loadProses = false;
                } 
                else
                {
                    loadProses = true;
                }

                StateHasChanged();
                return;
            } 
            
            else
            {
                loadProses = true;
            }
        }
    }
}
