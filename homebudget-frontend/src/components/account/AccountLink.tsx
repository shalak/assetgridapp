import * as React from "react";
import { Link } from "react-router-dom";
import { routes } from "../../lib/routes";
import { Account } from "../../models/account";

export interface Props {
    account: Account;
    targetBlank?: boolean;
    disabled?: boolean;
}

export default function AccountLink(props: Props) {
    if (props.disabled) {
        return <span className="transaction-link">
            <span>#{props.account.id}</span> {props.account.name}
        </span>;
    }

    return <Link className="account-link" to={routes.account(props.account.id.toString())} target={props.targetBlank === true ? "_blank" : "_self"}>
        <span>#{props.account.id}</span> {props.account.name}
    </Link>;
}