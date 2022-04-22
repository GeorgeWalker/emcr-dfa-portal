﻿using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Resources.Payments;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace EMBC.Tests.Integration.ESS.Resources
{
    public class PaymentsTests : DynamicsWebAppTestBase
    {
        private readonly IPaymentRepository repository;

        public PaymentsTests(ITestOutputHelper output, DynamicsWebAppFixture fixture) : base(output, fixture)
        {
            repository = Services.GetRequiredService<IPaymentRepository>();
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task SavePayment_InteracPayment_Saved()
        {
            var linkedSupportIds = TestData.SupportIds;
            var payment = new InteracSupportPayment
            {
                Status = PaymentStatus.Pending,
                Amount = 100.00m,
                RecipientFirstName = "first",
                RecipientLastName = "last",
                NotificationEmail = "email@unit.test",
                NotificationPhone = "1234567890",
                SecurityAnswer = "answer",
                SecurityQuestion = "question",
                LinkedSupportIds = linkedSupportIds
            };

            var paymentId = ((SavePaymentResponse)await repository.Manage(new SavePaymentRequest { Payment = payment })).Id;
            paymentId.ShouldNotBeNullOrEmpty();

            var readPayment = ((SearchPaymentResponse)await repository.Query(new SearchPaymentRequest { ById = paymentId })).Items
                .ShouldHaveSingleItem()
                .ShouldBeOfType<InteracSupportPayment>();

            readPayment.Id.ShouldBe(paymentId);
            readPayment.Status.ShouldBe(payment.Status);
            readPayment.RecipientFirstName.ShouldBe(payment.RecipientFirstName);
            readPayment.RecipientLastName.ShouldBe(payment.RecipientLastName);
            readPayment.NotificationEmail.ShouldBe(payment.NotificationEmail);
            readPayment.NotificationPhone.ShouldBe(payment.NotificationPhone);
            readPayment.SecurityAnswer.ShouldBe(payment.SecurityAnswer);
            readPayment.SecurityQuestion.ShouldBe(payment.SecurityQuestion);
            readPayment.LinkedSupportIds.ShouldBe(payment.LinkedSupportIds);
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task SendPaymentToCas_InteracPayment_Sent()
        {
            var payments = new[]
            {
                new InteracSupportPayment
                    {
                        Status = PaymentStatus.Pending,
                        Amount = 100.00m,
                        RecipientFirstName = "first",
                        RecipientLastName = "last",
                        NotificationEmail = "email@unit.test",
                        NotificationPhone = "1234567890",
                        SecurityAnswer = "answer",
                        SecurityQuestion = "question",
                        LinkedSupportIds = TestData.SupportIds
                    }
            };

            foreach (var payment in payments)
            {
                payment.Id = ((SavePaymentResponse)await repository.Manage(new SavePaymentRequest { Payment = payment })).Id;
            }

            var sentPayments = ((SendPaymentToCasResponse)await repository.Manage(new SendPaymentToCasRequest
            {
                Items = payments.Select(p => new CasPayment { PaymentId = p.Id })
            })).Items.ToArray();
            sentPayments.Length.ShouldBe(payments.Length);

            foreach (var paymentId in sentPayments)
            {
                var payment = ((SearchPaymentResponse)await repository.Query(new SearchPaymentRequest { ById = paymentId })).Items.ShouldHaveSingleItem();
                payment.Status.ShouldBe(PaymentStatus.Sent);
            }
        }
    }
}
