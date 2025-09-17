import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faBoltLightning, faTextWidth, faWarning } from '@fortawesome/free-solid-svg-icons';
import { useNavigate } from "react-router-dom";
import Button from "react-bootstrap/esm/Button";
import { useTranslation, Trans } from "react-i18next";

export default function Header() {

    const { t } = useTranslation();
    let navigate = useNavigate();

    return (
        <>
            <div className="container py-5 my-4 bg-light text-dark text-center">
                <div className="row justify-content-center mb-4">

                    <div className="lc-block col-xl-8">
                        <h1 className="display-2 fw-bold">
                            <Trans i18nKey="header_main_title">
                                Connect. Sell. Grow. Be part, here your sales boost the success of <span className="text-danger">everyone</span>.
                            </Trans>
                        </h1>
                    </div>


                </div>
                <div className="row justify-content-center mb-4">

                    <div className="lc-block col-xl-6 lh-lg">
                        <div>
                            <p>{t('header_subtitle')}</p>
                        </div>
                    </div>

                </div>
                <div className="row pb-4">
                    <div className="col-md-12">
                        <div className="lc-block d-grid gap-3 d-md-block">
                            <Button variant="danger" size="lg" className="me-md-2" onClick={() => {
                                navigate("/new-seller");
                            }}><FontAwesomeIcon icon={faBoltLightning} fixedWidth />{t('be_a_representative')}</Button>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}