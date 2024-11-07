function pnrData(){
    let pnr = JSON.parse(localStorage.getItem('pnr')) ?? [];
    return pnr;
}

function showinAdmin(){
    let showAll = document.getElementById('showAll');
    let pnr = pnrData();
    console.log(pnr);
    let html = ''
    html = `
    <div class="card col-6 mb-3" style="max-width: 540px">
    <div class="card-body text-center">
        <h5 class="card-title fs-3"></h5>
        <p class="card-text fs-3">Resigestered Users</p>
    </div>
</div>
    `
}
