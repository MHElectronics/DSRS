export function InitMap() {
    document.querySelector('#svgMap').addEventListener('click', function (evt) {
        var e = evt.target;
        var dim = e.getBoundingClientRect();
        var x = evt.clientX - dim.left;
        var y = evt.clientY - dim.top;
        alert("x: "+x+" y:"+y);
    });
}
