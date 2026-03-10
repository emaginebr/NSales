import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Form from 'react-bootstrap/Form';
import AuthContext from "../../Contexts/Auth/AuthContext";
import Button from "react-bootstrap/esm/Button";
import Card from 'react-bootstrap/Card';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faAddressBook, faArrowLeft, faArrowRight, faBitcoinSign, faCalendar, faCalendarAlt, faCancel, faClose, faCode, faDollar, faEnvelope, faEthernet, faIdCard, faLock, faPercent, faPhone, faSave, faSignInAlt, faTrash, faUpload, faUser } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate } from "react-router-dom";
import InputGroup from 'react-bootstrap/InputGroup';
import UserContext from "../../Contexts/User/UserContext";
import MessageToast from "../../Components/MessageToast";
import Moment from 'moment';
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import { ImageModal, ImageTypeEnum } from "../../Components/ImageModal";
import { useTranslation } from "react-i18next";

export default function NetworkEditPage() {

    const { t } = useTranslation();


    const authContext = useContext(AuthContext);
    //const userContext = useContext(UserContext);
    const networkContext = useContext(NetworkContext);


    const [dialog, setDialog] = useState<MessageToastEnum>(MessageToastEnum.Error);
    const [showMessage, setShowMessage] = useState<boolean>(false);
    const [messageText, setMessageText] = useState<string>("");

    const [showImageModal, setShowImageModal] = useState<boolean>(false);

    let navigate = useNavigate();
    Moment.locale('en');

    const throwError = (message: string) => {
        setDialog(MessageToastEnum.Error)
        setMessageText(message);
        setShowMessage(true);
    };
    const showSuccessMessage = (message: string) => {
        setDialog(MessageToastEnum.Success)
        setMessageText(message);
        setShowMessage(true);
    };

    useEffect(() => {
        if (authContext.sessionInfo && networkContext.userNetwork) {
            networkContext.getById(networkContext.userNetwork.networkId).then((ret) => {
                if (!ret.sucesso) {
                    throwError(ret.mensagemErro);
                }
            });
        }
    }, []);

    return (
        <>
            <MessageToast
                dialog={dialog}
                showMessage={showMessage}
                messageText={messageText}
                onClose={() => setShowMessage(false)}
            ></MessageToast>
            <ImageModal
                Image={ImageTypeEnum.Network}
                networkId={networkContext.network?.networkId}
                show={showImageModal}
                onClose={() => setShowImageModal(false)}
                onSuccess={(url: string) => {
                    networkContext.setNetwork({
                        ...networkContext.network,
                        imageUrl: url
                    });
                }}
            />
            <Container>
                <Row>
                    <Col md="12">
                        <Card>
                            <Card.Body>
                                <Row>
                                    <Col md={4} className="text-center">
                                        <div className="mb-2">
                                            {networkContext.network?.imageUrl &&
                                                <img src={networkContext.network?.imageUrl} style={{ width: "100%", height: "auto" }} />
                                            }
                                        </div>
                                        <div className="lc-block d-grid gap-3 d-md-block">
                                            {networkContext.network?.networkId > 0 &&
                                                <Button variant="primary" className="me-md-2" size="lg" onClick={async (e) => {
                                                    e.preventDefault();
                                                    setShowImageModal(true);
                                                }}>
                                                    <FontAwesomeIcon icon={faUpload} fixedWidth />&nbsp;{t('network_edit_change_image_button')}
                                                </Button>
                                            }
                                        </div>
                                    </Col>
                                    <Col md={8}>
                                        <Form>
                                            <div className="text-center mb-3"> {/* TODO: This text seems out of place for a network edit page. Confirm if it should be translated or removed/changed. */}
                                                {t('network_edit_registration_info')}
                                            </div>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">Name:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="text" size="lg"
                                                            placeholder={t('network_edit_name_placeholder')}
                                                            value={networkContext.network?.name}
                                                            onChange={(e) => {
                                                                networkContext.setNetwork({
                                                                    ...networkContext.network,
                                                                    name: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">Slug:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faCode} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="text" size="lg"
                                                            placeholder={t('network_edit_slug_placeholder')}
                                                            value={networkContext.network?.slug}
                                                            onChange={(e) => {
                                                                networkContext.setNetwork({
                                                                    ...networkContext.network,
                                                                    slug: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">Email:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faEnvelope} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="email" size="lg"
                                                            placeholder={t('network_edit_email_placeholder')}
                                                            value={networkContext.network?.email}
                                                            onChange={(e) => {
                                                                networkContext.setNetwork({
                                                                    ...networkContext.network,
                                                                    email: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t('network_edit_minimal_withdrawal_label')}:</Form.Label>
                                                <Col sm="4">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faDollar} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="number" size="lg"
                                                            placeholder={t('network_edit_minimal_withdrawal_placeholder')}
                                                            value={networkContext.network?.withdrawalMin}
                                                            onChange={(e) => {
                                                                networkContext.setNetwork({
                                                                    ...networkContext.network,
                                                                    withdrawalMin: parseFloat(e.target.value)
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                                <Form.Label column sm="2">{t('network_edit_withdrawal_period_label')}:</Form.Label>
                                                <Col sm="4">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faCalendar} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="number" size="lg"
                                                            placeholder={t('network_edit_withdrawal_period_placeholder')}
                                                            value={networkContext.network?.withdrawalPeriod}
                                                            onChange={(e) => {
                                                                networkContext.setNetwork({
                                                                    ...networkContext.network,
                                                                    withdrawalPeriod: parseInt(e.target.value)
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t('network_edit_commission_label')}:</Form.Label>
                                                <Col sm="4">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faPercent} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="number" size="lg"
                                                            placeholder={t('network_edit_commission_placeholder')}
                                                            value={networkContext.network?.comission}
                                                            onChange={(e) => {
                                                                networkContext.setNetwork({
                                                                    ...networkContext.network,
                                                                    comission: parseInt(e.target.value)
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                                <Button variant="danger" size="lg" onClick={() => {
                                                    navigate("/admin/dashboard"); // TODO: Consider if this route should be dynamic or translated if it's a label
                                                }}><FontAwesomeIcon icon={faArrowLeft} fixedWidth /> {t('back_button')}</Button>
                                                <Button variant="success" size="lg" onClick={async (e) => {
                                                    let ret = await networkContext.update(networkContext.network);
                                                    if (ret.sucesso) {
                                                        //alert(userContext.user?.id);
                                                        showSuccessMessage(t('network_edit_update_success_message'));
                                                    }
                                                    else {
                                                        throwError(ret.mensagemErro);
                                                    }
                                                }}
                                                    disabled={networkContext.loadingUpdate}
                                                >
                                                    {networkContext.loadingUpdate ? t('loading') :
                                                        <>
                                                            <FontAwesomeIcon icon={faSave} fixedWidth />&nbsp;{t('save_button')}
                                                        </>}
                                                </Button>
                                            </div>
                                        </Form>
                                    </Col>
                                </Row>
                            </Card.Body>
                        </Card>
                    </Col>
                </Row>
            </Container>
        </>
    );
}