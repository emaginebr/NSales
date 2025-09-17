import { CheckoutProvider, Elements, EmbeddedCheckout, EmbeddedCheckoutProvider, PaymentElement } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';
import { useContext, useEffect } from "react";
import OrderContext from "../../Contexts/Order/OrderContext";

const stripePromise = loadStripe('pk_test_51QkuslD37qwDaRRTa9aljJbNC73hIl0kznGiY5d8kHrvxtNrbyg3YxVufK3KQZeLLTjK6UNVeulQ74JUz0Nda4ZR00vo6t2EML');

interface ProductPaymentParam {
    productSlug: string;
    networkSlug?: string;
    sellerSlug?: string;
}

export default function SubscriptionForm(param: ProductPaymentParam) {

    const orderContext = useContext(OrderContext);

    const fetchClientSecret = async () => {
        let ret = await orderContext.createSubscription(param.productSlug, param.networkSlug, param.sellerSlug);
        if (ret.sucesso) {
            return ret.clientSecret;
        }
        else {
            alert(ret.mensagemErro);
            return "";
        }
    };

    return (
        <>
            <div className="card">
                <div className="card-header">
                    <h4 className="my-0">Payment</h4>
                </div>
                <div className="card-body text-center">
                    <EmbeddedCheckoutProvider stripe={stripePromise} options={{ fetchClientSecret }}>
                        <EmbeddedCheckout />
                    </EmbeddedCheckoutProvider>
                </div>
            </div>
        </>
    );
}