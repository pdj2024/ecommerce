using WebShared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net.Http;
using static System.Net.Http.IHttpClientFactory;

namespace Services
{
    public partial class InvoiceDocument : IDocument
    {

        private readonly Pesanan _pesanan;
        private readonly Company _company;
        private readonly IHttpClientFactory _http;

        public InvoiceDocument(Pesanan data, Company company, IHttpClientFactory http)
        {
            _pesanan = data;
            _company = company;
            _http = http;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            try 
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Background()
                    .PaddingVertical(30)
                    .AlignMiddle()
                    .Rotate(30)
                    .Decoration(decoration =>
                    {
                        decoration.Before().Element(DrawSide);
                        decoration.Content().Extend();
                        decoration.After().Element(DrawSide);

                        void DrawSide(IContainer container)
                        {
                            container
                                .Height(1000, Unit.Millimetre)
                                .AlignMiddle()
                                .Row(row =>
                                {
                                    row.AutoItem().PaddingRight(16).Text(_pesanan.Data.Header.FirstOrDefault()?.Status_Order_1.ToUpper()).FontSize(16).FontColor(Colors.Red.Medium);
                                });
                        }
                    });

                    page.Header().Column(col =>
                    {
                        col.Item().Background("#1500b2").Padding(15).Row(row =>
                        {
                            row.RelativeItem().Text("INVOICE")
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.White);

                            row.ConstantItem(300).AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text($"{_company.No_Telp}").FontColor(Colors.White);
                                c.Item().AlignRight().Text($"{_company.Email} | https://pacificdatajaya.com/").FontColor(Colors.White);
                                c.Item().AlignRight().PaddingTop(7);
                                c.Item().AlignRight().Text($"{_company.Alamat_Toko}, Kec. Gubeng").FontColor(Colors.White);
                                c.Item().AlignRight().Text($"Surabaya, Jawa Timur, Indonesia ({_company.Kode_Pos})").FontColor(Colors.White);
                            });
                        });

                        col.Item().PaddingVertical(8);
                    });

                    page.Content().Column(col =>
                    {

                        col.Item().Row(row =>
                        {
                            // ---- BILLED TO ----
                            row.ConstantItem(200).PaddingRight(10).Column(c =>
                            {
                                c.Item().Text("Penerima").FontColor(Color.FromHex("#999999"));
                                c.Item().Text(_pesanan.Data.Header.FirstOrDefault()?.Nama_Penerima).Bold();
                                c.Item().Text(_pesanan.Data.Header.FirstOrDefault()?.Telpon_Penerima);
                                c.Item().Text(_pesanan.Data.Header.FirstOrDefault()?.Alamat_Kirim);
                                //c.Item().Text($"{_pesanan.Data.Header.FirstOrDefault()?.Kecamatan}, {_pesanan.Data.Header.FirstOrDefault()?.Kota}, {_pesanan.Data.Header.FirstOrDefault()?.Provinsi}, {_pesanan.Data.Header.FirstOrDefault()?.Kode_Pos}");
                                //c.Item().Text(_pesanan.Data.Header.FirstOrDefault()?.Detil_Kirim_Lain);
                            });

                            // ---- INVOICE INFO ----
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Nomor Pesanan").FontColor(Color.FromHex("#999999"));
                                c.Item().Text($"{_pesanan.Data.Header.FirstOrDefault()?.No_Tracking}").Bold();

                                c.Item().PaddingTop(5);
                                c.Item().Text("Tanggal").FontColor(Color.FromHex("#999999"));
                                c.Item().Text($"{_pesanan.Data.Header.FirstOrDefault()?.Created_At.ToString("dd/MM/yyyy HH:mm")}");
                            });

                            // ---- TOTAL ----
                            row.ConstantItem(160).AlignRight().Column(c =>
                            {
                                c.Item().Text("Total Nota").FontColor(Color.FromHex("#999999"));
                                c.Item().Text($"{_pesanan.Data.Header.FirstOrDefault()?.Grand_Total.ToString("N0")}")
                                    .FontSize(22)
                                    .Bold()
                                    .FontColor("#1500b2");
                            });
                        });

                        col.Item().PaddingVertical(8);
                        // ================= TABLE =================
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(25);   // No
                                c.ConstantColumn(60);   // Gambar
                                c.RelativeColumn();     // Description
                                c.ConstantColumn(40);   // Qty
                                c.ConstantColumn(80);   // Unit Cost
                                c.ConstantColumn(100);   // Amount
                            });

                            // ---- HEADER ----
                            table.Header(h =>
                            {
                                h.Cell().Background("#f7f6ff").PaddingVertical(5).AlignCenter().Text("No").Bold().FontColor("#1500b2");
                                h.Cell().Background("#f7f6ff").PaddingVertical(5).AlignCenter().Text("Barang").Bold().FontColor("#1500b2");
                                h.Cell().Background("#f7f6ff").PaddingVertical(5).Text("").Bold().FontColor("#1500b2");
                                h.Cell().Background("#f7f6ff").PaddingVertical(5).AlignCenter().Text("Qty").Bold().FontColor("#1500b2");
                                h.Cell().Background("#f7f6ff").PaddingVertical(5).AlignCenter().Text("Harga").Bold().FontColor("#1500b2");
                                h.Cell().Background("#f7f6ff").PaddingVertical(5).AlignRight().Text("Total").Bold().FontColor("#1500b2");
                            });

                            int no = 1;
                            foreach (var item in _pesanan.Data.Items)
                            {
                                table.Cell().BorderColor("#e4e0ff").BorderBottom(1).PaddingVertical(2).AlignMiddle().AlignCenter().PaddingVertical(5).Text($" {no++.ToString()}");

                                table.Cell().BorderColor("#e4e0ff").BorderBottom(1).PaddingVertical(2).AlignMiddle().Height(50).AlignCenter().AlignMiddle().Element(e =>
                                {
                                    if (item.Var_Id_Media != null)
                                        e.Image(item.Var_Id_Media_Byte).FitArea();
                                    else
                                        e.Text("-").AlignCenter();
                                    //e.Text("-").AlignCenter();
                                });

                                table.Cell().BorderColor("#e4e0ff").BorderBottom(1).PaddingVertical(2).AlignMiddle().PaddingVertical(5).Column(c =>
                                {
                                    c.Item().PaddingBottom(5).Text(item.Nama_Display).Bold();

                                    c.Item().Text($"{item.Var_Size_Label}, {item.Var_Option_Label}").FontColor(Color.FromHex("#999999")).FontSize(9);
                                });

                                table.Cell().BorderColor("#e4e0ff").BorderBottom(1).PaddingVertical(2).AlignMiddle().PaddingVertical(5).AlignCenter()
                                    .Text(item.Qty.ToString("N0"));

                                table.Cell().BorderColor("#e4e0ff").BorderBottom(1).PaddingVertical(2).AlignMiddle().PaddingVertical(5).AlignRight()
                                    .Text(item.Harga_Satuan.ToString("N0"));


                                table.Cell().BorderColor("#e4e0ff").BorderBottom(1).PaddingVertical(2).AlignMiddle().PaddingVertical(5).AlignRight()
                                    .Text($"{(item.Qty * item.Harga_Satuan).ToString("N0")} ").Bold();
                            }
                        });

                        col.Item().PaddingVertical(5);

                        col.Item().AlignBottom().Row(row =>
                        {
                            // ================= FOOTER =================
                            row.RelativeItem().AlignBottom().Column(c =>
                            {
                                c.Item().Text("Ketentuan Nota").FontColor(Color.FromHex("#999999"));
                                c.Item().Text("- Nota ini adalah bukti pemesanan yang sah");
                                c.Item().Text("- Segala bentuk pemalsuan akan dikenakan denda dan hukuman yang berlaku");
                            });

                            // ================= SUMMARY =================
                            row.ConstantItem(200).AlignBottom().AlignRight().Column(c =>
                            {
                                c.Item().PaddingBottom(3).Row(r =>
                                {
                                    r.RelativeItem().Text("Subtotal").AlignRight();
                                    r.ConstantItem(80).Text($"{_pesanan.Data.Header.FirstOrDefault()?.Tot_DPP.ToString("N0")}").Bold().AlignRight();
                                });
                                c.Item().PaddingBottom(3).Row(r =>
                                {
                                    r.RelativeItem().Text("Biaya Pengiriman").AlignRight();
                                    r.ConstantItem(80).Text($"{_pesanan.Data.Biaya.FirstOrDefault(x => x.Id_Variant == "Ongkir").Harga_Total.ToString("N0")}").Bold().AlignRight();
                                });
                                c.Item().PaddingBottom(3).Row(r =>
                                {
                                    r.RelativeItem().Text("Biaya Layanan").AlignRight();
                                    r.ConstantItem(80).Text($"{_pesanan.Data.Biaya.FirstOrDefault(x => x.Id_Variant == "Layanan").Harga_Total.ToString("N0")}").Bold().AlignRight();
                                });

                                if (!_pesanan.Data.Header.FirstOrDefault()?.Incl_PPN ?? true)
                                {
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text($"PPN ({_pesanan.Data.Header.FirstOrDefault()?.PPN}%)").AlignRight();
                                        r.ConstantItem(80).Text($"{_pesanan.Data.Header.FirstOrDefault()?.Tot_PPN.ToString("N0")}").Bold().AlignRight();
                                    });
                                }

                                c.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Total Pesanan")
                                        .Bold()
                                        .FontSize(13)
                                        .FontColor("#1500b2")
                                        .AlignRight();

                                    r.ConstantItem(80).Text($"{_pesanan.Data.Header.FirstOrDefault()?.Grand_Total.ToString("N0")}")
                                        .Bold()
                                        .FontSize(13)
                                        .FontColor("#1500b2")
                                        .AlignRight();
                                });
                            });
                        });
                       
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"{_company.Nama_Toko}")
                                .FontSize(10)
                                .Bold()
                                .FontColor("#1500b2");

                            row.ConstantItem(300).AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text($"{_company.Email}").FontColor("#1500b2");
                            });
                        });
                    });
                });
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


    }

}