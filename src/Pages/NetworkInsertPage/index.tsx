import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Form from 'react-bootstrap/Form';
import AuthContext from "../../Contexts/Auth/AuthContext";
import Button from "react-bootstrap/esm/Button";
import Card from 'react-bootstrap/Card';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faArrowLeft, faArrowRight, faCancel, faEnvelope, faLock, faPercent, faUser } from '@fortawesome/free-solid-svg-icons';
import { Link, useNavigate } from "react-router-dom";
import InputGroup from 'react-bootstrap/InputGroup';
import UserContext from "../../Contexts/User/UserContext";
import MessageToast from "../../Components/MessageToast";
import Moment from 'moment';
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import NetworkInsertInfo from "../../DTO/Domain/NetworkInsertInfo";
import { useTranslation } from "react-i18next";

export default function NetworkInsertPage() {

    const { t } = useTranslation();


    const authContext = useContext(AuthContext);
    const userContext = useContext(UserContext);
    const networkContext = useContext(NetworkContext);

    const [step, setStep] = useState<number>(1);
    const [email, setEmail] = useState<string>("");
    const [password, setPassword] = useState<string>("");
    const [confirmPassword, setConfirmPassword] = useState<string>("");

    const [networkName, setNetworkName] = useState<string>("");
    const [networkEmail, setNetworkEmail] = useState<string>(authContext.sessionInfo?.email);
    const [networkCommission, setNetworkCommission] = useState<number>(0);

    const [dialog, setDialog] = useState<MessageToastEnum>(MessageToastEnum.Error);
    const [showMessage, setShowMessage] = useState<boolean>(false);
    const [messageText, setMessageText] = useState<string>("");

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
        if (authContext.sessionInfo) {
            if (authContext.sessionInfo?.userId > 0) {
                /*
                userContext.getMe().then((ret) => {
                    if (ret.sucesso) {
                        setStep(2);
                    }
                    else {
                        setStep(1);
                    }
                });
                */
                setStep(2);
            }
            else {
                setStep(1);
            }
        }
        else {
            setStep(1);
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
            <Container>
                <Row>
                    <Col md={12} className="text-center">
                        <ul id="progressbar" className="text-center align-items-center">
                            <li className="active" id="step1"><div className="d-none d-md-block">{t('network_insert_step_register_user')}</div></li>
                            <li className={step > 1 ? "active" : ""} id="step2"><div className="d-none d-md-block">{t('network_insert_step_register_network')}</div></li>
                            <li className={step > 2 ? "active" : ""} id="step3"><div className="d-none d-md-block">{t('network_insert_step_payment')}</div></li>
                            <li className={step > 3 ? "active" : ""} id="step4"><div className="d-none d-md-block">{t('network_insert_step_done')}</div></li>
                        </ul>
                    </Col>
                </Row>
            </Container>
            {step == 1 &&
                <>
                    <Container className="mb-5">
                        <Row>
                            <Col md="6">
                                <Card>
                                    <Card.Header>
                                        <h3 className="text-center">{t('login_title')}</h3>
                                    </Card.Header>
                                    <Card.Body>
                                        <Form>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="3">{t('login_email_label')}:</Form.Label>
                                                <Col sm="9">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="email" size="lg" placeholder={t('login_email_placeholder')} value={email} onChange={(e) => {
                                                            setEmail(e.target.value);
                                                        }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="3">{t('login_password_label')}:</Form.Label>
                                                <Col sm="9">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="password" size="lg" placeholder={t('login_password_placeholder')} value={password} onChange={(e) => {
                                                            setPassword(e.target.value);
                                                        }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Col sm="9" className="offset-sm-3">
                                                    <Form.Check type="checkbox" label={t('login_remember_password_label')} />
                                                </Col>
                                            </Form.Group>
                                            <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                                <Button variant="danger" size="lg" onClick={() => {
                                                    navigate("/recovery-password");
                                                }}><FontAwesomeIcon icon={faEnvelope} fixedWidth /> {t('login_recovery_password_button')}</Button>
                                                <Button variant="success" size="lg" disabled={authContext.loading} onClick={async (e) => {
                                                    e.preventDefault();
                                                    if (!email) {
                                                        throwError(t('login_error_email_empty'));
                                                        return;
                                                    }
                                                    if (!password) {
                                                        throwError(t('login_error_password_empty'));
                                                        return;
                                                    }
                                                    let ret = await authContext.loginWithEmail(email, password);
                                                    if (ret.sucesso) {
                                                        let netRet = await networkContext.listByUser();
                                                        if (netRet.sucesso) {
                                                            setStep(2);
                                                        }
                                                        else {
                                                            throwError(netRet.mensagemErro);
                                                        }
                                                    }
                                                    else {
                                                        throwError(ret.mensagemErro);
                                                    }
                                                }}>
                                                    {authContext.loading ? t('loading') : t('next_button')}
                                                    <FontAwesomeIcon icon={faArrowRight} fixedWidth />
                                                </Button>
                                            </div>
                                        </Form>
                                    </Card.Body>
                                </Card>
                            </Col>
                            <Col md="6">
                                <Card>
                                    <Card.Header>
                                        <h3 className="text-center">{t('network_insert_user_registration_title')}</h3>
                                    </Card.Header>
                                    <Card.Body>
                                        <Form>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t('form_label_name')}:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="text" size="lg"
                                                            placeholder={t('form_placeholder_your_name')}
                                                            value={userContext.user?.name}
                                                            onChange={(e) => {
                                                                userContext.setUser({
                                                                    ...userContext.user,
                                                                    name: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t('form_label_email')}:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faEnvelope} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="text" size="lg"
                                                            placeholder={t('form_placeholder_your_email')}
                                                            value={userContext.user?.email}
                                                            onChange={(e) => {
                                                                userContext.setUser({
                                                                    ...userContext.user,
                                                                    email: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t('form_label_password')}:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="password" size="lg"
                                                            placeholder={t('form_placeholder_your_password')}
                                                            value={userContext.user?.password}
                                                            onChange={(e) => {
                                                                userContext.setUser({
                                                                    ...userContext.user,
                                                                    password: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t('form_label_confirm_password')}:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="password" size="lg"
                                                            placeholder={t('form_placeholder_confirm_your_password')}
                                                            value={confirmPassword}
                                                            onChange={(e) => {
                                                                setConfirmPassword(e.target.value);
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                                <Button variant="success" size="lg" onClick={async (e) => {
                                                    if (!userContext.user?.name) {
                                                        throwError(t('error_name_empty'));
                                                        return;
                                                    }
                                                    if (!userContext.user?.email) {
                                                        throwError(t('error_email_empty'));
                                                        return;
                                                    }
                                                    if (userContext.user?.password != confirmPassword) {
                                                        throwError(t('error_password_confirmation_different'));
                                                        return;
                                                    }
                                                    let ret = await userContext.insert(userContext.user);
                                                    if (ret.sucesso) {
                                                        showSuccessMessage(ret.mensagemSucesso);
                                                        //alert(userContext.user?.id);
                                                        setStep(2);
                                                    }
                                                    else {
                                                        throwError(ret.mensagemErro);
                                                    }
                                                }}
                                                    disabled={userContext.loadingUpdate}
                                                > {userContext.loadingUpdate ? t('loading') : t('next_button')}
                                                    <FontAwesomeIcon icon={faArrowRight} fixedWidth />
                                                </Button>
                                            </div>
                                        </Form>
                                    </Card.Body>
                                </Card>
                            </Col>
                        </Row>
                    </Container>
                </>
            }
            {step == 2 &&
                <Container>
                    <Row>
                        <Col md="12">
                            <Card>
                                <Card.Header>
                                    <h3 className="text-center">{t('network_insert_network_registration_title')}</h3>
                                </Card.Header>
                                <Card.Body>
                                    <Form>
                                        <div className="text-center mb-3">
                                            {t('network_edit_registration_info')} {/* Using existing key, confirm if appropriate */}
                                        </div>
                                        <Form.Group as={Row} className="mb-3">
                                            <Form.Label column sm="2">{t('form_label_name')}:</Form.Label>
                                            <Col sm="10">
                                                <InputGroup>
                                                    <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                                    <Form.Control type="text" size="lg"
                                                        placeholder={t('network_edit_name_placeholder')}
                                                        value={networkName}
                                                        onChange={(e) => {
                                                            setNetworkName(e.target.value);
                                                        }} />
                                                </InputGroup>
                                            </Col>
                                        </Form.Group>
                                        <Form.Group as={Row} className="mb-3">
                                            <Form.Label column sm="2">{t('form_label_email')}:</Form.Label>
                                            <Col sm="10">
                                                <InputGroup>
                                                    <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                                    <Form.Control type="email" size="lg"
                                                        placeholder={t('network_insert_network_email_placeholder')}
                                                        value={networkEmail}
                                                        onChange={(e) => {
                                                            setNetworkEmail(e.target.value);
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
                                                        value={networkCommission}
                                                        onChange={(e) => {
                                                            setNetworkCommission(parseFloat(e.target.value));
                                                        }}
                                                    />
                                                </InputGroup>
                                            </Col>
                                        </Form.Group>
                                        <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                            <Button variant="danger" size="lg" onClick={() => {
                                                setStep(2);
                                            }}><FontAwesomeIcon icon={faArrowLeft} fixedWidth /> {t('back_button')}</Button>
                                            <Button variant="success" size="lg" onClick={async (e) => {
                                                let networkInsert: NetworkInsertInfo;
                                                let ret = await networkContext.insert({
                                                    ...networkInsert,
                                                    name: networkName,
                                                    email: networkEmail,
                                                    comission: networkCommission,
                                                    plan: 1
                                                });
                                                if (ret.sucesso) {
                                                    showSuccessMessage(ret.mensagemSucesso);
                                                    setStep(4);
                                                    let retUsers = await networkContext.listByUser();
                                                    if (!retUsers.sucesso) {
                                                        throwError(retUsers.mensagemErro);
                                                    }
                                                    //alert(userContext.user?.id);
                                                }
                                                else {
                                                    throwError(ret.mensagemErro);
                                                }
                                            }}
                                                disabled={networkContext.loadingUpdate}
                                            >
                                                {networkContext.loadingUpdate ? t('loading') :
                                                    <>
                                                        {t('next_button')}&nbsp;<FontAwesomeIcon icon={faArrowRight} fixedWidth />
                                                    </>}
                                            </Button>
                                        </div>
                                    </Form>
                                </Card.Body>
                            </Card>
                        </Col>
                    </Row>
                </Container>
            }
            {step == 4 &&
                <Container>
                    <Row>
                        <Col md="12">
                            <Card>
                                <Card.Body className="text-center">
                                    <h1>{t('network_insert_success_title')}</h1>
                                    <p>{t('network_insert_success_message_1')}</p>
                                    <p>{t('network_insert_success_message_2')}</p>
                                    <p className="text-center">{t('network_insert_success_lets_start')}</p>
                                    <Button variant="primary" size="lg" onClick={(e) => {
                                        e.preventDefault();
                                        navigate("/admin/dashboard");
                                    }}>{t('network_insert_access_my_network_button')} <FontAwesomeIcon icon={faArrowRight} fixedWidth /></Button>
                                </Card.Body>
                            </Card>
                        </Col>
                    </Row>
                </Container>
            }
        </>
    );
}