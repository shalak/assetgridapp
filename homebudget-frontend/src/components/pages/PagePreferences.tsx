import axios from "axios";
import * as React from "react";
import { Link } from "react-router-dom";
import { routes } from "../../lib/routes";
import { formatNumber } from "../../lib/Utils";
import { Preferences } from "../../models/preferences";
import { Card } from "../common/Card";
import InputButton from "../form/InputButton";
import InputNumber from "../form/InputNumber";
import InputText from "../form/InputText";

interface Props {
    preferences: Preferences | "fetching",
    updatePreferences: () => void,
}

interface State {
    preferences: Preferences | "fetching",
}

export default class PagePreferences extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            preferences: "fetching"
        }
    }

    public componentDidMount(): void {
        if (this.props.preferences !== "fetching") {
            this.setState({ preferences: this.props.preferences });
        }
    }

    public componentDidUpdate(prevProps: Readonly<Props>, prevState: Readonly<State>): void {
        if (this.props.preferences !== "fetching" && prevProps.preferences === "fetching") {
            this.setState({ preferences: this.props.preferences });
        }
    }

    public render() {
        if (this.state.preferences === "fetching") {
            return "Please wait";
        }

        return <div className="section container">
            <section className="hero has-background-primary">
                <div className="hero-body">
                    <p className="title has-text-white">
                        Preferences
                    </p>
                </div>
            </section>

            <Card title="Number formatting">
                <div className="columns">
                    <div className="column">
                    <InputText value={this.state.preferences.decimalSeparator}
                        label="Decimal Separator"
                        onChange={(event => this.setState({
                            preferences: {
                                ...this.state.preferences as Preferences,
                                decimalSeparator: event.target.value
                            }
                        }))} />
                    </div>
                    <div className="column">
                    <InputText value={this.state.preferences.thousandsSeparator}
                        label="Thousands Separator"
                        onChange={(event => this.setState({
                            preferences: {
                                ...this.state.preferences as Preferences,
                                thousandsSeparator: event.target.value
                            }
                        }))} />
                    </div>
                    <div className="column">
                        <InputNumber value={this.state.preferences.decimalDigits}
                            label="Digits after decimal point"
                            onChange={(event => this.setState({
                                preferences: {
                                    ...this.state.preferences as Preferences,
                                    decimalDigits: event.target.valueAsNumber
                                }
                            }))} />
                    </div>
                </div>
                
                <p>Example: {
                    formatNumber(123456789.123456789,
                        this.state.preferences.decimalDigits,
                        this.state.preferences.decimalSeparator,
                        this.state.preferences.thousandsSeparator)}
                </p>
                
                <InputButton className="is-primary" onClick={this.SaveChanges.bind(this)}>
                    Save changes
                </InputButton>
            </Card>
        </div>;
    }

    private SaveChanges() {
        if (this.state.preferences === "fetching") {
            throw "error";
        }
        let preferences = this.state.preferences;

        this.setState({ preferences: "fetching" }, () => {
            axios.put<Preferences>('https://localhost:7262/user/preferences', preferences)
                .then(res => {
                    this.setState({ preferences: res.data });
                    this.props.updatePreferences();
                })
                .catch(e => {
                    console.log(e);
                    this.setState({ preferences: "fetching" });
                });
        });
    }
}