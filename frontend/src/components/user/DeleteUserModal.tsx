import * as React from "react";
import { useNavigate } from "react-router";
import { useApi } from "../../lib/ApiClient";
import Modal from "../common/Modal";
import InputButton from "../input/InputButton";

interface Props {
    active: boolean;
    close: () => void;
}

export default function DeleteUserModal(props: Props) {
    const [isDeleting, setisDeleting] = React.useState(false);
    const api = useApi();
    const navigate = useNavigate();

    return <Modal
        active={props.active}
        title={"Delete user"}
        close={() => props.close()}
        footer={<>
            {<InputButton onClick={() => deleteUser()} disabled={isDeleting || api === null} className="is-danger">Delete user</InputButton>}
            <button className="button" onClick={() => props.close()}>Cancel</button>
        </>}>
        <p>Are you sure you want to delete your user account? This action is irreversible!</p>
    </Modal>;

    async function deleteUser() {
        if (api === null) return;

        setisDeleting(true);
        //await api.User.delete(props.transaction.id);
        // Refresh the page
        navigate(0);
    }
}