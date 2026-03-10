import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Form from 'react-bootstrap/Form';
import AuthContext from "../../Contexts/Auth/AuthContext";
import Button from "react-bootstrap/esm/Button";
import Card from 'react-bootstrap/Card';
import InputGroup from 'react-bootstrap/InputGroup';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faArrowRight, faEnvelope, faLock, faRightFromBracket, faUser, faUserAlt } from "@fortawesome/free-solid-svg-icons";
import { useContext, useState } from "react";
import { useTranslation } from "react-i18next";
import { createSearchParams, useNavigate } from "react-router-dom";
import UserContext from "../../Contexts/User/UserContext";

interface IUserParam {
    url: string;
    onThrowError: (msgErro: string) => void;
    onSuccess: (msgSuccess: string) => void;
};

export default function UserForm(param: IUserParam) {

    const { t } = useTranslation();

    const [email, setEmail] = useState<string>("");
    const [password, setPassword] = useState<string>("");
    const [confirmPassword, setConfirmPassword] = useState<string>("");

    const authContext = useContext(AuthContext);
    const userContext = useContext(UserContext);

    let navigate = useNavigate();

    return (
        <>

            <Card>
                <Card.Header>
                    <h3 className="text-center">{t('user_form_registration_title')}</h3>
                </Card.Header>
                <Card.Body>
                    <Form>
                        <Form.Group className="mb-1">
                            <Form.Label sm="2">{t('form_label_name')}:</Form.Label>
                            <InputGroup>
                                <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                <Form.Control type="text"
                                    placeholder={t('form_placeholder_your_name')}
                                    value={userContext.user?.name}
                                    onChange={(e) => {
                                        userContext.setUser({
                                            ...userContext.user,
                                            name: e.target.value
                                        });
                                    }} />
                            </InputGroup>
                        </Form.Group>
                        <Form.Group className="mb-1">
                            <Form.Label sm="2">{t('form_label_email')}:</Form.Label>
                            <InputGroup>
                                <InputGroup.Text><FontAwesomeIcon icon={faEnvelope} fixedWidth /></InputGroup.Text>
                                <Form.Control type="text"
                                    placeholder={t('form_placeholder_your_email')}
                                    value={userContext.user?.email}
                                    onChange={(e) => {
                                        userContext.setUser({
                                            ...userContext.user,
                                            email: e.target.value
                                        });
                                    }} />
                            </InputGroup>
                        </Form.Group>
                        <Form.Group className="mb-1">
                            <Form.Label sm="2">{t('form_label_password')}:</Form.Label>
                            <InputGroup>
                                <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                <Form.Control type="password"
                                    placeholder={t('form_placeholder_your_password')}
                                    value={userContext.user?.password}
                                    onChange={(e) => {
                                        userContext.setUser({
                                            ...userContext.user,
                                            password: e.target.value
                                        });
                                    }} />
                            </InputGroup>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label sm="2">{t('form_label_confirm_password')}:</Form.Label>
                            <InputGroup>
                                <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                <Form.Control type="password"
                                    placeholder={t('form_placeholder_confirm_your_password')}
                                    value={confirmPassword}
                                    onChange={(e) => {
                                        setConfirmPassword(e.target.value);
                                    }} />
                            </InputGroup>
                        </Form.Group>
                        <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                            <Button variant="danger" onClick={() => {
                                navigate({
                                    pathname: "/account/login",
                                    search: createSearchParams({
                                        returnUrl: param.url
                                    }).toString()
                                });
                            }}><FontAwesomeIcon icon={faRightFromBracket} fixedWidth /> {t('sign_in_button')}</Button>
                            <Button variant="success" onClick={async (e) => {
                                if (!userContext.user?.name) {
                                    param.onThrowError(t('error_name_empty'));
                                    return;
                                }
                                if (!userContext.user?.email) {
                                    param.onThrowError(t('error_email_empty'));
                                    return;
                                }
                                if (userContext.user?.password != confirmPassword) {
                                    param.onThrowError(t('error_password_confirmation_different'));
                                    return;
                                }
                                let ret = await userContext.insert(userContext.user);
                                if (ret.sucesso) {
                                    param.onSuccess(ret.mensagemSucesso);
                                }
                                else {
                                    param.onThrowError(ret.mensagemErro);
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
        </>
    );
}