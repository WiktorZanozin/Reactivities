import React, { useContext } from 'react'
import {RouteProps} from 'react-router'
import { RouteComponentProps, Redirect, Route } from 'react-router-dom'
import { RootStoreContext } from '../stores/rootStore';
import { observer } from 'mobx-react-lite';

interface IProps extends RouteProps{
component:React.ComponentType<RouteComponentProps<any>>
}
const PrivateRoute: React.FC<IProps> = ({component:Component, ...rest}) => {
    const rootStore=useContext(RootStoreContext);
    const {IsLoggedIn}=rootStore.userStore;
    return (
      <Route
      {...rest}
      render={(props)=>IsLoggedIn?<Component {...props}/>:<Redirect to={'/'}/>}
      
      />
    )
}
export default observer(PrivateRoute);