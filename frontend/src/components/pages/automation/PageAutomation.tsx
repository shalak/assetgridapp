import { faTrashCan } from "@fortawesome/free-regular-svg-icons";
import { faPen, faPersonRunning } from "@fortawesome/free-solid-svg-icons";
import * as React from "react";
import { Link } from "react-router-dom";
import { Api, useApi } from "../../../lib/ApiClient";
import { routes } from "../../../lib/routes";
import { forget } from "../../../lib/Utils";
import { TransactionAutomationPermissions, TransactionAutomationSummary } from "../../../models/automation/transactionAutomation";
import Card from "../../common/Card";
import Hero from "../../common/Hero";
import Modal from "../../common/Modal";
import Table from "../../common/Table";
import Tooltip from "../../common/Tooltip";
import InputButton from "../../input/InputButton";
import InputIconButton from "../../input/InputIconButton";
import YesNoDisplay from "../../input/YesNoDisplay";

export default function PageAutomation (): React.ReactElement {
    const api = useApi();
    const [isUpdating, setIsUpdating] = React.useState(false);
    const [transactionAutomations, setTransactionAutomations] = React.useState<TransactionAutomationSummary[]>([]);
    const [transactionAutomationPage, setTransactionAutomationPage] = React.useState(1);
    const [transactionAutomationDraw, setTransactionAutomationDraw] = React.useState(1);
    const [transactionAutomationDeleting, setTransactionAutomationDeleting] = React.useState<TransactionAutomationSummary | null>(null);

    React.useEffect(() => {
        if (api !== null) forget(refreshLists)(api);
    }, [api]);

    return <>
        <Hero title="Automation" />
        <div className="p-3">
            <Card title="Transaction automations" isNarrow={false}>
                <Table pageSize={10}
                    renderItem={(automation, i) => <tr key={i}>
                        <td>{automation.name}</td>
                        <td>{automation.description}</td>
                        <td><YesNoDisplay value={automation.enabled} /></td>
                        <td>
                            {automation.permissions === TransactionAutomationPermissions.Modify && <>
                                <Tooltip content="Run automation on existing transactions">
                                    <Link to={routes.automationTransactionEdit(automation.id.toString())} state={{ expandPreview: true }}>
                                        <InputIconButton disabled={isUpdating} icon={faPersonRunning} />
                                    </Link>
                                </Tooltip>
                                <Link to={routes.automationTransactionEdit(automation.id.toString())}>
                                    <InputIconButton disabled={isUpdating} icon={faPen} />
                                </Link>
                                <InputIconButton disabled={isUpdating} icon={faTrashCan} onClick={() => setTransactionAutomationDeleting(automation)} />
                            </>}
                        </td>
                    </tr>}
                    page={transactionAutomationPage}
                    goToPage={setTransactionAutomationPage}
                    draw={transactionAutomationDraw}
                    type="sync"
                    renderType="table"
                    headings={<tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Enabled</th>
                        <th>Actions</th>
                    </tr>} items={transactionAutomations} />
                <div className="buttons">
                    <Link to={routes.automationTransactionCreate()} className="button is-primary">Create transaction automation</Link>
                </div>
            </Card>
        </div>
        <Modal
            active={transactionAutomationDeleting !== null}
            title="Delete automation"
            close={() => setTransactionAutomationDeleting(null)}
            footer={<>
                <InputButton disabled={isUpdating} className="is-danger" onClick={() => forget(deleteTransactionAutomation)(transactionAutomationDeleting?.id)}>Delete</InputButton>
                <InputButton disabled={isUpdating} onClick={() => setTransactionAutomationDeleting(null)}>Cancel</InputButton>
            </>}>
            Are you sure you want to delete the automation &ldquo;{transactionAutomationDeleting?.name}&rdquo; with the description &ldquo;{transactionAutomationDeleting?.description}&rdquo;?
            This action is ireversible
        </Modal>
    </>;

    async function refreshLists (api: Api): Promise<void> {
        const result = await api.Automation.Transaction.list();
        if (result.status === 200) {
            setTransactionAutomations(result.data);
            setTransactionAutomationDraw(draw => draw + 1);
        }
    }

    async function deleteTransactionAutomation (id: number): Promise<void> {
        if (api === null) return;

        setIsUpdating(true);
        const result = await api.Automation.Transaction.delete(id);
        if (result.status === 200) {
            await refreshLists(api);
        }
        setIsUpdating(false);
        setTransactionAutomationDeleting(null);
    }
}