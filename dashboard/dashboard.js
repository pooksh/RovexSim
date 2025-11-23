document.addEventListener("DOMContentLoaded", () => {

const agvCountSelect = document.getElementById("agvCount");
const agvCardsDiv = document.getElementById("agvCards");
let agvCount = Number(agvCountSelect.value);
const MAX_POINTS = 20;
let agvs = [];

// Chart helper
function createChart(ctx,labelName,color){
  return new Chart(ctx,{
    type:"line",
    data:{
      labels:[],
      datasets:[{
        label: labelName,
        data: [],
        borderColor: color,
        backgroundColor: color + "44",
        fill: true,
        tension: 0.4, // smoother lines
        pointRadius: 4,
        pointHoverRadius: 6
      }]
    },
    options:{
      responsive:true,
      maintainAspectRatio:false,
      animation:{ duration: 500 },
      plugins:{
        legend:{
          labels:{
            color:"#fff",
            font:{size:16, family:"'Inter', sans-serif", weight:"600"}
          }
        }
      },
      scales:{
        x:{
          ticks:{color:"#fff", font:{size:14, family:"'Inter', sans-serif", weight:"500"}},
          grid:{color:"#2c2c2c"}
        },
        y:{
          ticks:{color:"#fff", font:{size:14, family:"'Inter', sans-serif", weight:"500"}},
          grid:{color:"#2c2c2c"}
        }
      }
    }
  });
}

// Main 2x2 charts
let avgTimeChart = createChart(document.getElementById("avgTimeChart"),"Avg Task Time","#3b82f6");
let tasksHourChart = createChart(document.getElementById("tasksHourChart"),"Tasks/Hour","#6366f1");
let distanceChart = createChart(document.getElementById("distanceChart"),"Distance (m)","#8b5cf6");
let errorRateChart = createChart(document.getElementById("errorRateChart"),"Error Rate (%)","#a78bfa");

function initAGVs(count){
  agvs = [];
  agvCardsDiv.innerHTML = "";
  for(let i=0;i<count;i++){
    const agv = {
      id:i+1,
      tasksCompleted: Math.floor(Math.random()*5) + 5,
      avgTime: Math.random()*10+5,
      distance: Math.random()*500+200,
      errorRate: Math.random()*0.2,
      history:{avgTime:[], tasksCompleted:[], distance:[], errorRate:[]}
    };
    for(const key in agv.history) agv.history[key].push(agv[key]);

    const card = document.createElement("div");
    card.classList.add("agv-card");
    card.id = `agvCard${agv.id}`;
    card.innerHTML = `
      <h3>AGV ${agv.id}</h3>
      <p id="tasks${agv.id}">Tasks: ${agv.tasksCompleted}</p>
      <p id="time${agv.id}">Avg Time: ${agv.avgTime.toFixed(1)} min</p>
      <p id="dist${agv.id}">Distance: ${agv.distance.toFixed(0)} m</p>
      <p id="err${agv.id}">Error Rate: ${(agv.errorRate*100).toFixed(1)}%</p>
    `;
    agvCardsDiv.appendChild(card);

    agvs.push(agv);
  }
}

initAGVs(agvCount);

agvCountSelect.addEventListener("change", ()=>{
  agvCount = Number(agvCountSelect.value);
  initAGVs(agvCount);
});

function updateDashboard(){
  const timestamp = new Date().toLocaleTimeString();
  let totalAvgTime=0, totalTasks=0, totalDistance=0, totalError=0;

  agvs.forEach(a=>{
    a.avgTime = Math.max(2, a.avgTime + (Math.random()*2-1));
    a.tasksCompleted += Math.random() < 0.5 ? 1 : 0;
    a.distance = Math.max(100, a.distance + (Math.random()*20-10));
    a.errorRate = Math.min(1, Math.max(0, a.errorRate + (Math.random()*0.02-0.01)));

    for(const key in a.history){
      const value = key==="errorRate"? a.errorRate*100 : a[key];
      a.history[key].push(value);
      if(a.history[key].length>MAX_POINTS) a.history[key].shift();
    }

    document.getElementById(`tasks${a.id}`).innerText = `Tasks: ${a.tasksCompleted}`;
    document.getElementById(`time${a.id}`).innerText = `Avg Time: ${a.avgTime.toFixed(1)} min`;
    document.getElementById(`dist${a.id}`).innerText = `Distance: ${a.distance.toFixed(0)} m`;
    document.getElementById(`err${a.id}`).innerText = `Error Rate: ${(a.errorRate*100).toFixed(1)}%`;

    totalAvgTime += a.avgTime;
    totalTasks += a.tasksCompleted;
    totalDistance += a.distance;
    totalError += a.errorRate;
  });

  // overall KPI
  document.getElementById("avgTime").innerText = (totalAvgTime/agvs.length).toFixed(1)+" min";
  document.getElementById("tasksHour").innerText = (totalTasks/agvs.length).toFixed(1);
  document.getElementById("distance").innerText = (totalDistance/1000).toFixed(2)+" km";
  document.getElementById("labor").innerText = "$"+((totalAvgTime/60*25).toFixed(2));
  document.getElementById("errorRate").innerText = ((totalError/agvs.length)*100).toFixed(1)+"%";

  // main charts
  function pushData(chart,data){
    chart.data.labels.push(timestamp);
    chart.data.datasets[0].data.push(data);
    if(chart.data.labels.length>MAX_POINTS){
      chart.data.labels.shift();
      chart.data.datasets[0].data.shift();
    }
    chart.update();
  }
  pushData(avgTimeChart, totalAvgTime/agvs.length);
  pushData(tasksHourChart, totalTasks/agvs.length);
  pushData(distanceChart, totalDistance);
  pushData(errorRateChart, (totalError/agvs.length)*100);
}

setInterval(updateDashboard,2000);
});
