function blazorInitializeModal(dialog, reference) {
    dialog.addEventListener("close", async e => {
        //await reference.invokeMethodAsync("OnClose", dialog.returnValue);
        await reference.invokeMethodAsync("Hide");
    });
}

function blazorOpenModal(dialog) {
    if (!dialog.open) {
        dialog.showModal();
    }
}

function blazorCloseModal(dialog) {
    if (dialog.open) {
        dialog.close();
    }
}
