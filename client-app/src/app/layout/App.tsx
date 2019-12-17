import React, {useState, useEffect, Fragment, SyntheticEvent} from 'react';
import {Header, Icon, List, Container} from 'semantic-ui-react';
import axios from 'axios';
import {IActivity} from '../models/activity'
import NavBar from '../../features/nav/NavBar';
import ActivityDashboard from '../../features/activities/dashboard/ActivityDashboard';
import ActivityForm from '../../features/activities/form/ActivityForm';
import agent from '../api/agent'
import { LoadingComponent } from './LoadingComponent';

const App =()=> {
  const[activities,setActivities]=useState<IActivity[]>([])
  const[selectedActivity, setSelectedActivity]=useState<IActivity | null>(null)
  const[editMode, setEditMode]=useState(false);
  const[loading, setLoading]=useState(true);
  const[submitting, setSubmiting]=useState(false);
  const[target, setTarget]=useState('');

  const handleSelectedActivity=(id:string)=>{
    setSelectedActivity(activities.filter(a=>a.id===id)[0]);
    setEditMode(false);
  }
  const handleOpenCreateForm=()=>{
    setSelectedActivity(null);
    setEditMode(true);
  }
  const handleCreateActivity=(activity:IActivity)=>{
    setSubmiting(true);
    agent.Activities.create(activity).then(()=>{
    setActivities([...activities, activity])
    setSelectedActivity(activity);
    setEditMode(false);
  }).then(()=>setSubmiting(false))
}
  const handleEditActivity=(activity:IActivity)=>{
    setSubmiting(true);
    agent.Activities.update(activity).then(()=>{
    setActivities([...activities.filter(a=>a.id!==activity.id),activity])
    setSelectedActivity(activity);
  }).then(()=>setSubmiting(false))
}

  const handleDeleteActivity=(event:SyntheticEvent<HTMLButtonElement>,id:string)=>{
    setSubmiting(true);
    setTarget(event.currentTarget.name);
    agent.Activities.delete(id).then(()=>{
    setActivities([...activities.filter(a=>a.id!==id)])
  }).then(()=>setSubmiting(false))
}
  useEffect(()=>{
   agent.Activities.list()
  .then(response=>{
      let activities:IActivity[]=[];
      response.forEach((activity:any)=>{
      activity.date=activity.date.split('.')[0];
      activities.push(activity);
      })
       setActivities(activities)
      }).then(()=>setLoading(false));
      }, []);
 
      if(loading) return <LoadingComponent content='Loading activities...'/>
  return (
    <Fragment>
      <NavBar openCreateForm={handleOpenCreateForm}/>
      <Container style={{marginTop:'7em'}}>
        <ActivityDashboard activities={activities} 
        selectActivity={handleSelectedActivity}
        selectedActivity={selectedActivity}
        editMode={editMode}
        setEditMode={setEditMode}
        setSelectedActivity={setSelectedActivity}
        createActivity={handleCreateActivity}
        editActivity={handleEditActivity}
        deleteActivity={handleDeleteActivity}
        submitting={submitting}
        target={target}
       />
      </Container>
    </Fragment>
  );
}


export default App;
