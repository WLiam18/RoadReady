using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace RoadReady.BookingService.Documents;

public class BookingReceiptDocument : IDocument
{
    private readonly ReceiptData _data;

    public BookingReceiptDocument(ReceiptData data)
    {
        _data = data;
    }

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial).FontColor("#1a1a1a"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.BorderBottom(2).BorderColor("#1a1a1a").PaddingBottom(15).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("ROADREADY").FontSize(20).Bold().LetterSpacing(0.05f);
                column.Item().Text("PAYMENT RECEIPT").FontSize(9).FontColor("#666666").LetterSpacing(0.05f);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BILLED TO").Style(SectionTitleStyle());
                    col.Item().Text(_data.CustomerName).Bold();
                    col.Item().Text(_data.CustomerEmail);
                    col.Item().Text(_data.CustomerPhone);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BOOKING REFERENCE").Style(SectionTitleStyle());
                    col.Item().Text(_data.BookingReference).Bold();
                    
                    col.Item().PaddingTop(10).Text("DATE OF ISSUE").Style(SectionTitleStyle());
                    col.Item().Text(_data.IssueDate.ToString("MMMM dd, yyyy"));
                });
            });

            column.Item().PaddingTop(30).Text("RENTAL ITINERARY").Style(SectionTitleStyle());
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("VEHICLE").Style(LabelStyle());
                    col.Item().Text(_data.VehicleMakeModel).Bold();
                    
                    col.Item().PaddingTop(10).Text("LOCATION").Style(LabelStyle());
                    col.Item().Text(_data.Location);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PICKUP").Style(LabelStyle());
                    col.Item().Text(_data.PickupDate.ToString("MMMM dd, yyyy \u2022 HH:mm 'UTC'"));
                    
                    col.Item().PaddingTop(10).Text("DROPOFF").Style(LabelStyle());
                    col.Item().Text(_data.DropoffDate.ToString("MMMM dd, yyyy \u2022 HH:mm 'UTC'"));
                });
            });

            column.Item().PaddingTop(35).Text("FINANCIAL BREAKDOWN").Style(SectionTitleStyle());
            column.Item().Element(ComposeTable);
            
            column.Item().PaddingTop(10).Element(ComposeTotals);
        });
    }

    private void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); 
                columns.RelativeColumn(1); 
                columns.RelativeColumn(1); 
                columns.RelativeColumn(1); 
            });

            table.Header(header =>
            {
                header.Cell().BorderBottom(1).BorderColor("#eeeeee").PaddingBottom(5).Text("DESCRIPTION").Style(SectionTitleStyle());
                header.Cell().BorderBottom(1).BorderColor("#eeeeee").PaddingBottom(5).AlignRight().Text("RATE").Style(SectionTitleStyle());
                header.Cell().BorderBottom(1).BorderColor("#eeeeee").PaddingBottom(5).AlignRight().Text("QTY").Style(SectionTitleStyle());
                header.Cell().BorderBottom(1).BorderColor("#eeeeee").PaddingBottom(5).AlignRight().Text("AMOUNT").Style(SectionTitleStyle());
            });

            foreach (var item in _data.Items)
            {
                table.Cell().PaddingVertical(8).BorderBottom(1).BorderColor("#eeeeee").Text(item.Description);
                table.Cell().PaddingVertical(8).BorderBottom(1).BorderColor("#eeeeee").AlignRight().Text($"₹{item.Rate:N2}");
                table.Cell().PaddingVertical(8).BorderBottom(1).BorderColor("#eeeeee").AlignRight().Text($"{item.Quantity} {item.Unit}");
                table.Cell().PaddingVertical(8).BorderBottom(1).BorderColor("#eeeeee").AlignRight().Text($"₹{item.Amount:N2}");
            }
        });
    }

    private void ComposeTotals(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem(); 
            row.RelativeItem().Column(column =>
            {
                column.Item().PaddingVertical(3).Row(r =>
                {
                    r.RelativeItem().AlignRight().Text("Subtotal").FontColor("#666666");
                    r.ConstantItem(80).AlignRight().Text($"₹{_data.Subtotal:N2}");
                });
                
                column.Item().PaddingVertical(3).Row(r =>
                {
                    r.RelativeItem().AlignRight().Text("Taxes (18% GST)").FontColor("#666666");
                    r.ConstantItem(80).AlignRight().Text($"₹{_data.Taxes:N2}");
                });
                
                column.Item().PaddingTop(10).BorderTop(2).BorderColor("#1a1a1a").PaddingTop(10).Row(r =>
                {
                    r.RelativeItem().AlignRight().Text("Total Paid").FontSize(12).Bold();
                    r.ConstantItem(80).AlignRight().Text($"₹{_data.TotalPaid:N2}").FontSize(12).Bold();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text("THANK YOU FOR CHOOSING ROADREADY. DRIVE SAFELY.")
            .FontSize(8).FontColor("#999999").LetterSpacing(0.05f);
    }

    private TextStyle SectionTitleStyle() => TextStyle.Default.FontSize(8).FontColor("#888888").Bold().LetterSpacing(0.05f);
    private TextStyle LabelStyle() => TextStyle.Default.FontSize(8).FontColor("#888888").Bold();
}