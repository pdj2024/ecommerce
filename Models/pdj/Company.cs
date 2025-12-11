using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Models
{
    public class Navbar
    {
        public int Priority { get; set; } = 0;
        public string Nav_Type { get; set; } = "";
        public string Display_Text { get; set; } = "";
        public string Id_Company { get; set; } = "";
        public List<string> Filter { get; set; } = new();
        public List<string> Id_Brand { get; set; } = new();
        public List<Brand> Brand { get; set; } = new();
        public List<string> Id_Kategori { get; set; } = new();
        public List<Kategori> Kategori { get; set; } = new();
        public List<Produk> Produk { get; set; } = new();
    }

    public class CompanySosial
    {
        public string Id_Company { get; set; } = "";
        public string Social_Type { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Url { get; set; } = "";
        public string Media_Type { get; set; } = "";
        public string Id_Media { get; set; } = "";
        public string No_Id_Media { get; set; } = "";
    }

    public class TextSection
    {
        public string No_Id { get; set; } = "";
        public string Section_Name { get; set; } = "";
        public string Judul { get; set; } = "";
        public string Keterangan { get; set; } = "";
        public string Extra { get; set; } = "";
    }

    public class NilaiDefault
    {
        public int Limit_Produk_Home { get; set; } = 12;
        public int Limit_Produk_Search { get; set; } = 12;
        public decimal Ppn { get; set; } = 0;
        public decimal Expiry_In_Day { get; set; } = 0;
        public decimal Expiry_In_Hour { get; set; } = 0;
        public decimal Expiry_In_Minute { get; set; } = 0;
        public bool Incl_Ppn { get; set; } = false;
        public string Id_Company { get; set; } = "";
    }

    public class SourchData
    {
        public List<DataLogin> ListDataLogin { get; set; } = new List<DataLogin>();
        public List<Company> ListDataCompany { get; set; } = new List<Company>();
        public List<Banner> ListDataBanner { get; set; } = new List<Banner>();
        public List<Brand> ListDataBrand { get; set; } = new List<Brand>();
        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public List<Navbar> ListDataNavbar { get; set; } = new List<Navbar>();
        public List<DataUser> ListDataUser { get; set; } = new List<DataUser>();
        public List<Pembayaran> ListDataPembayaran { get; set; } = new List<Pembayaran>();
        public List<Pengiriman> ListDataPengiriman { get; set; } = new List<Pengiriman>();
        public List<NilaiDefault> ListDataNilaiDefault { get; set; } = new List<NilaiDefault>();
        public List<CompanySosial> ListDataCompanySocial { get; set; } = new List<CompanySosial>();
        public List<Cart> ListDataCart { get; set; } = new List<Cart>();
        public List<TextSection> ListDataTexkSection { get; set; } = new List<TextSection>();
        public List<Pesanan.SummaryResponse> ListDataSummaryPesanan { get; set; } = new List<Pesanan.SummaryResponse>();




        public NilaiDefault DataNilaiDefault { get; set; } = new NilaiDefault();
        public Company DataCompany { get; set; } = new Company();
        public CompanySosial DataCompanySocial { get; set; } = new CompanySosial();
        public Produk DataProduk { get; set; } = new Produk();
        public Cart DataCart { get; set; } = new Cart();
        public Brand DataBanner { get; set; } = new Brand();
        public DataUser DataUser { get; set; } = new DataUser();
        public Brand DataBrand { get; set; } = new Brand();
        public Pesanan DataPesanan { get; set; } = new Pesanan();
        public Pesanan DataPesananBelum { get; set; } = new Pesanan();
        public Pesanan DataPesananDikemas { get; set; } = new Pesanan();
        public Pesanan DataPesananDikirim { get; set; } = new Pesanan();
        public Pesanan DataPesananHistori { get; set; } = new Pesanan();
        public Navbar DataNavbar { get; set; } = new Navbar();
        public Pembayaran DataPembayaran { get; set; } = new Pembayaran();
        public Pengiriman DataPengiriman { get; set; } = new Pengiriman();
        public Pesanan.PaymentResponse DataResponsePesanan { get; set; } = new Pesanan.PaymentResponse();


        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount, bool hasMore) SearchList { get; set; } = (new(), new(), 0, false);

        public bool Finised { get; set; } = false;


        public readonly Dictionary<Type, (string, bool, DateTime)> ListPropertyMap = new()
        {
            { typeof(TextSection), (nameof(ListDataTexkSection), false, new DateTime())},
            { typeof(Company), (nameof(ListDataCompany), false, new DateTime())},
            { typeof(Banner), (nameof(ListDataBanner) , false, new DateTime())},
            { typeof(Brand), (nameof(ListDataBrand), false, new DateTime())},
            { typeof(Produk), (nameof(ListDataProduk), false, new DateTime())},
            { typeof(Kategori), (nameof(ListDataKategori), false, new DateTime())},
            { typeof(Cart), (nameof(ListDataCart), false, new DateTime())},
            { typeof(Navbar), (nameof(ListDataNavbar), false, new DateTime())},
            { typeof(DataUser), (nameof(ListDataUser), false, new DateTime()) },
            { typeof(Pembayaran), (nameof(ListDataPembayaran), false, new DateTime()) },
            { typeof(Pengiriman), (nameof(ListDataPengiriman), false, new DateTime())},
            { typeof(NilaiDefault), (nameof(ListDataNilaiDefault), false, new DateTime())},
            { typeof(CompanySosial), (nameof(ListDataCompanySocial), false, new DateTime())},
            { typeof(Pesanan.SummaryResponse), (nameof(ListDataSummaryPesanan), false, new DateTime())},
        };


        public void SetListData(Type type, object value)
        {
            if (value == null)
            {
                writeLog($"Tipe {type.Name} Kosong ListPropertyMap.");
                return;
            }

            if (ListPropertyMap.TryGetValue(type, out var propName))
            {
                var prop1 = GetType().GetProperty(propName.Item1);
                prop1.SetValue(this, value);

                ListPropertyMap[type] = (propName.Item1, true, DateTime.Now);
            }
            else
            {
                writeLog($"Tipe {type.Name} belum terdaftar dalam ListPropertyMap.");
            }
        }

        //public void SetSingleData(Type type, object value)
        //{
        //    if (value == null)
        //    {
        //        writeLog($"Tipe {type.Name} Kosong SinglePropertyMap.");
        //        return;
        //    }

        //    if (SinglePropertyMap.TryGetValue(type, out var propName))
        //    {
        //        var prop = GetType().GetProperty(propName);
        //        prop?.SetValue(this, value);
        //    }
        //    else
        //    {
        //        writeLog($"Tipe {type.Name} belum terdaftar dalam SinglePropertyMap.");
        //    }
        //}


        public object? GetListData(Type type)
        {
            if (ListPropertyMap.TryGetValue(type, out var propName))
            {
                var prop = GetType().GetProperty(propName.Item1);
                return prop?.GetValue(this);
            }
            else
            {
                writeLog($"Tipe {type.Name} Kosong ListPropertyMap.");
                return null;
            }
        }

        //public object? GetSingleData(Type type)
        //{
        //    if (SinglePropertyMap.TryGetValue(type, out var propName))
        //    {
        //        var prop = GetType().GetProperty(propName);
        //        return prop?.GetValue(this);
        //    }
        //    else
        //    {
        //        writeLog($"Tipe {type.Name} Kosong SinglePropertyMap.");
        //        return null;
        //    }
        //}

        public object? GetSingleDataById(Type type, object no_id)
        {
            if (ListPropertyMap.TryGetValue(type, out var propName))
            {
                // Ambil list dari property yang sesuai tipe
                var prop = GetType().GetProperty(propName.Item1);
                var listObj = prop?.GetValue(this);

                if (listObj is IEnumerable<object> list)
                {
                    // Cari item yang punya property "no_id" sesuai dengan parameter
                    var item = list.FirstOrDefault(x => x.GetType().GetProperty("no_id").GetValue(x).ToString() == no_id.ToString());

                    if (item == null)
                        writeLog($"Data dengan no_id '{no_id}' tidak ditemukan di {type.Name}.");

                    return item;
                }
                else
                {
                    writeLog($"Properti {propName} bukan IEnumerable<object> untuk tipe {type.Name}.");
                    return null;
                }
            }
            else
            {
                writeLog($"Tipe {type.Name} tidak ditemukan di ListPropertyMap.");
                return null;
            }
        }


        public object? AddSingleDataById(Type type, object dataCurrent)
        {
            if (ListPropertyMap.TryGetValue(type, out var propName))
            {
                var prop = GetType().GetProperty(propName.Item1);
                var listObj = prop?.GetValue(this);

                if (listObj is IList list)
                {
                    list.Insert(0, dataCurrent); // ✅ tambahkan item baru
                    return dataCurrent;
                }

                writeLog($"Properti {propName} bukan IList untuk tipe {type.Name}.");
                return null;
            }
            else
            {
                writeLog($"Tipe {type.Name} tidak ditemukan di ListPropertyMap.");
                return null;
            }
        }

        public object? SetSingleDataById(Type type, object no_id, object dataCurrent)
        {
            if (ListPropertyMap.TryGetValue(type, out var propName))
            {
                var prop = GetType().GetProperty(propName.Item1);
                var listObj = prop?.GetValue(this);

                if (listObj is IList list) // ubah ke IList agar bisa akses index
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i];
                        var propId = obj.GetType().GetProperty("no_id");
                        if (propId?.GetValue(obj)?.ToString() == no_id.ToString())
                        {
                            list[i] = dataCurrent; // ini yang benar-benar update list
                            return dataCurrent;
                        }
                    }

                    writeLog($"Data dengan no_id '{no_id}' tidak ditemukan di {type.Name}.");
                    return null;
                }
                else
                {
                    writeLog($"Properti {propName} bukan IList untuk tipe {type.Name}.");
                    return null;
                }
            }
            else
            {
                writeLog($"Tipe {type.Name} tidak ditemukan di ListPropertyMap.");
                return null;
            }
        }

        public object? DelSingleDataById(Type type, object no_id, object dataCurrent)
        {
            if (ListPropertyMap.TryGetValue(type, out var propName))
            {
                var prop = GetType().GetProperty(propName.Item1);
                var listObj = prop?.GetValue(this);

                if (listObj is IList list) // ubah ke IList agar bisa akses index
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i];
                        var propId = obj.GetType().GetProperty("no_id");
                        if (propId?.GetValue(obj)?.ToString() == no_id.ToString())
                        {
                            dataCurrent = list[i]; // ini yang benar-benar update list
                            list.RemoveAt(i);
                            return dataCurrent;
                        }
                    }

                    writeLog($"Data dengan no_id '{no_id}' tidak ditemukan di {type.Name}.");
                    return null;
                }
                else
                {
                    writeLog($"Properti {propName} bukan IList untuk tipe {type.Name}.");
                    return null;
                }
            }
            else
            {
                writeLog($"Tipe {type.Name} tidak ditemukan di ListPropertyMap.");
                return null;
            }
        }


        public async Task writeLog(string content)
        {
            var log = $@"
                        [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] - Error API Connection
                        Message: {content}
                        ---------------------------------------------------------";

            var logDir = Path.Combine("Logs");
            var fileName = $"LogApi-{DateTime.Now:yyyy-MM-dd}.txt";
            var logPath = Path.Combine(logDir, fileName);

            Directory.CreateDirectory(logDir);

            using (var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(log);
            }
        }

    }


    public class ScopedData
    {
        public DataLogin DataLogin { get; set; } = new();
        public Produk DataProduk { get; set; } = new();

        public Pesanan DataPesananBelum { get; set; } = new();
        public List<Pembayaran> ListDataPembayaran { get; set; } = new();
        public Pesanan DataPesananDikemas { get; set; } = new();
        public Pesanan DataPesananDikirim { get; set; } = new();
        public Pesanan DataPesananSelesai { get; set; } = new();
        public Pesanan DataPesananBatal { get; set; } = new();
        public Pesanan DataPesananDetail { get; set; } = new();

        public List<Cart> ListDataCart { get; set; } = new List<Cart>();
        public List<Pesanan.SummaryResponse> ListDataSummaryPesanan { get; set; } = new List<Pesanan.SummaryResponse>();




        public bool Finised { get; set; } = false;
    }

     public partial class Company
    {
        public string No_Id { get; set; } = "";
        public string Nama_Toko { get; set; } = "Hallow All";
        public string Email { get; set; } = "";
        public string No_Telp { get; set; } = "";
        public string No_Wa_1 { get; set; } = "";
        public string No_Wa_2 { get; set; } = "";
        public string Alamat_Toko { get; set; } = "";
        public string Hari_Buka { get; set; } = "";
        public string Jam_Buka { get; set; } = "";
        public string Kode_Pos { get; set; } = "";
        public string About { get; set; } = "";
        public string Gmap { get; set; } = "";
        public List<CompanySosial> Media { get; set; } = new();
    }

}
