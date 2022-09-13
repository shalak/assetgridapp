import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Decimal from "decimal.js";
import { DateTime } from "luxon";
import React = require("react");
import { formatNumberWithPrefs } from "../../lib/Utils";
import { Account } from "../../models/account";
import { Preferences } from "../../models/preferences";
import { Transaction, TransactionLine } from "../../models/transaction";
import InputAccount from "../form/account/InputAccount";
import InputCategory from "../form/InputCategory";
import InputDate from "../form/InputDate";
import InputText from "../form/InputText";
import TransactionLink from "./TransactionLink";
import * as regular from "@fortawesome/free-regular-svg-icons"
import * as solid from "@fortawesome/free-solid-svg-icons"
import Modal from "../common/Modal";
import { Api } from "../../lib/ApiClient";
import AccountLink from "../account/AccountLink";
import InputButton from "../form/InputButton";
import InputNumber from "../form/InputNumber";
import Tooltip from "../common/Tooltip";
import InputIconButton from "../form/InputIconButton";

type Props  = {
    transaction: Transaction;
    preferences: Preferences | "fetching";
    updateItem: (item: Transaction) => void;
    accountId?: number;
    balance?: Decimal;
    allowEditing?: boolean;
    allowLinks?: boolean;
}

interface TransactionEditingModel {
    total: Decimal;
    description: string;
    dateTime: DateTime;
    source: Account | null;
    destination: Account | null;
    category: string;
    lines: TransactionLine[] | null;
};

export default function TransactionTableLine(props: Props) {
    const [disabled, setDisabled] = React.useState(false);
    const [state, setState] = React.useState<null | "editing" | "confirm-delete">(null);

    switch (state) {
        case "editing":
            return <TransactionEditor disabled={disabled} setDisabled={setDisabled} stopEditing={() => setState(null)} {...props} />;
        default:
            return <TableTransaction setState={setState} isConfirmingDeletion={state === "confirm-delete"} delete={deleteTransaction} disabled={disabled} {...props} />;
    }

    function deleteTransaction() {
        setDisabled(true);
        setState(null);

        Api.Transaction.delete(props.transaction.id).then(() => {
            setDisabled(false);
            props.updateItem(null);
        })
    }
}








/*
 * Transaction that is not currently being edited
 */
type TableTransactionProps = Props & {
    disabled: boolean;
    setState: (state: null | "editing" | "confirm-delete") => void;
    isConfirmingDeletion: boolean;
    delete: () => void;
}
function TableTransaction(props: TableTransactionProps) {
    const offsetAccount = props.accountId !== undefined ? props.transaction.destination?.id === props.accountId ? props.transaction.source : props.transaction.destination : null;
    const total = props.accountId === undefined || props.transaction.destination?.id === props.accountId ? props.transaction.total : props.transaction.total.neg();
    const totalClass = (total.greaterThan(0) && props.accountId !== undefined ? "positive" : (total.lessThan(0) && props.accountId !== undefined ? "negative" : ""));
    const [expandSplit, setExpandSplit] = React.useState(false);

    return <div key={props.transaction.id} className="table-row">
        <div>
            <TransactionLink transaction={props.transaction} disabled={! (props.allowLinks ?? true)} />
            {props.transaction.lines.length > 0 && <Tooltip
                content={expandSplit ? "This is a split transaction. Click to collapse." : "This is a split transaction. Click to expand."}>
                <InputIconButton icon={solid.faEllipsisVertical} onClick={() => setExpandSplit(expand => !expand)} />
            </Tooltip>}
        </div>
        <div>{props.transaction.dateTime.toString()}</div>
        <div>{props.transaction.description}</div>
        <div className={"number-total " + totalClass}>
            {formatNumberWithPrefs(total, props.preferences)}
        </div>
        {props.balance && <div className={"number-total"} style={{ fontWeight: "normal" }}>
            {formatNumberWithPrefs(props.balance, props.preferences)}
        </div>}
        <div>
            {props.accountId !== undefined
                ? offsetAccount !== null && <AccountLink account={offsetAccount} disabled={! (props.allowLinks ?? true)} />
                : props.transaction.source !== null && <AccountLink account={props.transaction.source} disabled={! (props.allowLinks ?? true)} />}
        </div>
        {props.accountId === undefined && <div>
            {props.transaction.destination !== null && <AccountLink account={props.transaction.destination} disabled={! (props.allowLinks ?? true)} />}
        </div>}
        <div>{props.transaction.category}</div>
        {props.allowEditing && <div>
            {!props.disabled && <>
                <InputIconButton icon={solid.faPen} onClick={() => props.setState("editing")} />
                <InputIconButton icon={regular.faTrashCan} onClick={() => props.setState("confirm-delete")} />
            </>}
            
            {/* Deletion modal */}
            {props.isConfirmingDeletion && <Modal
                active={true}
                title={"Delete transaction"}
                close={() => props.setState(null)}
                footer={<>
                    <button className="button is-danger" onClick={() => props.delete()}>Delete transaction</button>
                    <button className="button" onClick={() => props.setState(null)}>Cancel</button>
                </>}>
                Are you sure you want to delete transaction "#{props.transaction.id} {props.transaction.description}"?
            </Modal>}
        </div>}
        {expandSplit && < div className="transaction-lines split">
            {props.transaction.lines.map((line, i) => <div key={i} className={"transaction-line" + (i === props.transaction.lines.length - 1 ? " last" : "")}>
                <div style={{ gridColumn: "span 2" }}></div>
                <div className="description">
                    {line.description}
                </div>
                <div className="total">
                    {formatNumberWithPrefs(line.amount, props.preferences)}
                </div>
                <div style={{ gridColumn: "span 4" }}></div>
            </div>)}
        </div> }
    </div>;
}




/*
 * Inline transaction editor for transaction tables
 */
type TransactionEditorProps = Props & {
    disabled: boolean;
    setDisabled: (disabled: boolean) => void;
    stopEditing: () => void;
}

function TransactionEditor(props: TransactionEditorProps) {
    const defaultModel = {
        total: props.transaction.total,
        dateTime: props.transaction.dateTime,
        description: props.transaction.description,
        source: props.transaction.source,
        destination: props.transaction.destination,
        category: props.transaction.category,
        lines: props.transaction.lines.length > 0 ? props.transaction.lines : null,
    };
    const [model, setModel] = React.useState<TransactionEditingModel>(defaultModel);

    const total = props.accountId === undefined || props.transaction.destination?.id === props.accountId ? props.transaction.total : props.transaction.total.neg();
    const totalClass = (total.greaterThan(0) && props.accountId !== undefined ? "positive" : (total.lessThan(0) && props.accountId !== undefined ? "negative" : ""));
    // If the current account is the source, all amounts should be negative
    const amountMultiplier = props.accountId && props.accountId === props.transaction.source?.id ? new Decimal(-1) : new Decimal(1);

    return <div key={props.transaction.id} className="table-row editing">
        <div><TransactionLink transaction={props.transaction} /></div>
        <div>
            <InputDate value={model.dateTime}
                onChange={e => setModel({ ...model, dateTime: e })}
                disabled={props.disabled} /></div>
        <div>
            <InputText value={model.description}
                onChange={(e) => setModel({ ...model, description: e.target.value })}
                disabled={props.disabled} /></div>
        {model.lines === null
            ? <div>
                <InputNumber allowNull={false} value={model.total.times(amountMultiplier)} onChange={newTotal => setModel({ ...model, total: newTotal.times(amountMultiplier) })} />
            </div>
            : < div className={"number-total " + totalClass}>
                {/* If the transaction is split, the total is the sum of the lines */}
                {formatNumberWithPrefs(model.total.times(amountMultiplier), props.preferences)}
            </div>
        }
        {props.balance && <div className={"number-total"} style={{ fontWeight: "normal" }}>{formatNumberWithPrefs(props.balance, props.preferences)}</div>}
        {(props.accountId === undefined || props.accountId !== props.transaction.source?.id) && <div>
            <InputAccount
                value={model.source}
                disabled={props.disabled}
                allowNull={true}
                onChange={account => setModel({ ...model, source: account })} />
        </div>}
        {(props.accountId === undefined || props.accountId !== props.transaction.destination?.id) && <div>
            <InputAccount
                value={model.destination}
                disabled={props.disabled}
                allowNull={true}
                onChange={account => setModel({ ...model, destination: account })} />
        </div>}
        <div>
            <InputCategory
                value={model.category}
                disabled={props.disabled}
                onChange={category => setModel({ ...model, category: category })} />
        </div>
        <div>
            {! props.disabled && <>
                <InputIconButton icon={solid.faSave} onClick={() => saveChanges()} />
                <InputIconButton icon={solid.faXmark} onClick={() => props.stopEditing()} />
            </>}
        </div>
        {model.lines !== null
            ? < div className="transaction-lines split">
                {model.lines.map((line, i) => <TransactionLineEditor key={i}
                    line={line}
                    update={changes => updateLine(changes, i)}
                    delete={() => deleteLine(i)}
                    disabled={props.disabled}
                    inverse={amountMultiplier.toNumber() == -1}
                    last={i === model.lines.length} />)}
                <div style={{ gridColumn: "span 2"}}></div>
                <div className="btn-add-line">
                    <InputButton className="is-small"
                        onClick={() => setModel({
                            ...model,
                            lines: [...model.lines, { amount: new Decimal(0), description: "Transaction line" }]
                        })}>Add line</InputButton>
                </div>
                <div style={{ gridColumn: "span 4"}}></div>
            </div>
            : < div className="transaction-lines non-split">
                <InputButton className="is-small"
                    onClick={() => setModel({
                        ...model,
                        lines: [{ amount: props.transaction.total, description: "Transaction line" }]
                    })}>Split transaction</InputButton>
            </div> }
    </div>;

    function saveChanges() {
        props.setDisabled(true);

        if (model === null) {
            return;
        }

        Api.Transaction.update({
            id: props.transaction.id,
            dateTime: model.dateTime,
            description: model.description,
            sourceId: model.source?.id ?? -1,
            destinationId: model.destination?.id ?? -1,
            category: model.category,
            total: model.total,
            lines: model.lines ?? []
        }).then(result => {
            props.setDisabled(false);
            props.updateItem(result);
            props.stopEditing();
        });
    }

    function updateLine(newLine: Partial<TransactionLine>, index: number) {
        const lines = [
            ...model.lines.slice(0, index),
            { ...model.lines[index], ...newLine },
            ...model.lines.slice(index + 1),
        ];
        setModel({
            ...model,
            total: lines.reduce((sum, line) => sum.add(line.amount), new Decimal(0)),
            lines: lines
        })
    }

    function deleteLine(index: number) {
        const newLines = [...model.lines.slice(0, index), ...model.lines.slice(index + 1)];
        console.log(newLines);
        const total = newLines.length > 0 ? newLines.reduce((sum, line) => sum.add(line.amount), new Decimal(0)) : model.lines[index].amount;
        setModel({
            ...model,
            total: total,
            lines: newLines.length > 0 ? newLines : null
        });
    }
}

/*
 * Inline editor for a transaction line
 */
interface LineEditorProps {
    line: TransactionLine;
    update: (changes: Partial<TransactionLine>) => void;
    delete: () => void;
    disabled: boolean, last: boolean;
    inverse: boolean;
}
function TransactionLineEditor(props: LineEditorProps) {
    const multiplier = props.inverse ? new Decimal(-1) : new Decimal(1);

    return <div className={"transaction-line" + (props.last ? " last" : "")}>
        <div style={{ gridColumn: "span 2" }}>
            <InputIconButton icon={solid.faGripLinesVertical} onClick={() => 0} />
        </div>
        <div className="description">
            <InputText disabled={props.disabled}
                isSmall={true}
                value={props.line.description}
                onChange={e => props.update({ description: e.target.value })} />
        </div>
        <div className="total">
            <InputNumber
                disabled={props.disabled}
                allowNull={false}
                isSmall={true}
                value={props.line.amount.times(multiplier)}
                onChange={value => props.update({ amount: value.times(multiplier) })} />
        </div>
        <div style={{ gridColumn: "span 4" }}>
            <InputIconButton icon={regular.faTrashCan} onClick={props.delete} />
        </div>
    </div>;
}